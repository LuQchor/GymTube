using Microsoft.AspNetCore.Mvc;
using Stripe;
using GymTube.API.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace GymTube.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly string _webhookSecret;
        private readonly ILogger<WebhooksController> _logger;

        // Koristimo IOptions da dohvatimo konfiguraciju na siguran način
        public WebhooksController(IUserRepository userRepository, IConfiguration configuration, ILogger<WebhooksController> logger)
        {
            _userRepository = userRepository;
            _webhookSecret = configuration.GetValue<string>("Stripe:WebhookSecret") 
                ?? throw new ArgumentNullException("Stripe WebhookSecret is not configured.");
            _logger = logger;
        }

        [HttpPost("stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            _logger.LogInformation("Stripe webhook received.");

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _webhookSecret);
                
                _logger.LogInformation("Webhook signature validated. Event type: {EventType}", stripeEvent.Type);

                // Rukovanje događajem o uspješnom plaćanju
                if (stripeEvent.Type == "checkout.session.completed")
                {
                    var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    // Provjeravamo je li sesija za pretplatu i je li plaćena
                    if (session?.Mode == "subscription" && session.PaymentStatus == "paid")
                    {
                        var userId = session.ClientReferenceId;
                        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
                        if (user != null)
                        {
                            try
                            {
                                var subscriptionService = new SubscriptionService();
                                var subscription = await subscriptionService.GetAsync(session.SubscriptionId);
                                var invoiceService = new InvoiceService();
                                var invoice = await invoiceService.GetAsync(session.InvoiceId);
                                user.IsPremium = true;
                                user.StripeCustomerId = session.CustomerId; 
                                user.StripeSubscriptionId = session.SubscriptionId;
                                user.SubscriptionStatus = subscription.Status;
                                user.PremiumExpiresAt = subscription.Items.Data[0].CurrentPeriodEnd;
                                user.PlanType = subscription.Items.Data.FirstOrDefault()?.Price.Recurring?.Interval ?? "unknown";
                                await _userRepository.UpdateAsync(user);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error updating user {UserEmail} to premium status", user.Email);
                                return StatusCode(500, "Error updating user premium status");
                            }
                        }
                        else
                        {
                            _logger.LogWarning("User with ID {UserId} not found in database.", userId);
                            return NotFound("User not found");
                        }
                    }
                    else
                    {
                        _logger.LogInformation("Session {SessionId} is not a paid subscription (Mode: {Mode}, PaymentStatus: {PaymentStatus})", 
                            session?.Id, session?.Mode, session?.PaymentStatus);
                    }
                }
                
                // Rukovanje događajem o otkazivanju pretplate
                if (stripeEvent.Type == "customer.subscription.deleted")
                {
                    var subscription = stripeEvent.Data.Object as Stripe.Subscription;
                    
                    if (subscription != null)
                    {
                        // Pronađi korisnika s ovim subscription ID
                        var user = await _userRepository.GetByStripeSubscriptionIdAsync(subscription.Id);
                        if (user != null)
                        {
                            // Ažuriraj korisnika - koristimo datum isteka iz baze podataka
                            // Premium status ostaje aktivan do datuma isteka
                            var now = DateTime.UtcNow;
                            user.IsPremium = user.PremiumExpiresAt.HasValue && user.PremiumExpiresAt.Value > now;
                            // Ne mijenjamo PremiumExpiresAt - ostaje isti do kraja perioda
                            user.SubscriptionStatus = subscription.Status;

                            await _userRepository.UpdateAsync(user);
                        }
                    }
                }

                // Rukovanje događajem o uspješnom recurring plaćanju
                if (stripeEvent.Type == "invoice.payment_succeeded")
                {
                    var invoice = stripeEvent.Data.Object as Invoice;
                    var subscriptionId = invoice?.Lines.Data.FirstOrDefault()?.Subscription?.Id;
                    if (invoice != null && !string.IsNullOrEmpty(subscriptionId))
                    {
                        var user = await _userRepository.GetByStripeSubscriptionIdAsync(subscriptionId);
                        if (user != null)
                        {
                            user.IsPremium = true;
                            user.SubscriptionStatus = "active";
                            var subscriptionService2 = new SubscriptionService();
                            var subscription2 = await subscriptionService2.GetAsync(subscriptionId);
                            user.PremiumExpiresAt = subscription2.Items.Data[0].CurrentPeriodEnd;
                            await _userRepository.UpdateAsync(user);
                        }
                        else
                        {
                            _logger.LogWarning("User with Subscription ID {SubscriptionId} not found.", subscriptionId);
                        }
                    }
                }

                // Rukovanje događajem o neuspješnom recurring plaćanju
                if (stripeEvent.Type == "invoice.payment_failed")
                {
                    dynamic invoice = stripeEvent.Data.Object;
                    string subscriptionId = invoice.Subscription;
                    if (!string.IsNullOrEmpty(subscriptionId))
                    {
                        var user = await _userRepository.GetByStripeSubscriptionIdAsync(subscriptionId);
                        if (user != null)
                        {
                            var subscriptionService = new SubscriptionService();
                            var subscription = await subscriptionService.GetAsync(subscriptionId);
                            user.SubscriptionStatus = subscription.Status;
                            // user.IsPremium = false; // Odkomentiraj ako želiš odmah suspendirati
                            await _userRepository.UpdateAsync(user);
                        }
                    }
                }
                
                return Ok();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe webhook signature validation failed or other Stripe error.");
                // Neuspjela validacija potpisa ili druga Stripe greška
                return BadRequest(new { error = e.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred in the Stripe webhook.");
                return StatusCode(500, "Internal server error");
            }
        }

        // [HttpPost("check-expired-subscriptions")]
        // [AllowAnonymous]
        // public async Task<IActionResult> CheckExpiredSubscriptions()
        // {
        //     try
        //     {
        //         var expiredCount = 0;
        //         var now = DateTime.UtcNow;
        //         
        //         // Dohvati sve premium korisnike s isteklim datumom
        //         var expiredUsers = await _userRepository.GetExpiredPremiumUsersAsync(now);
        //         
        //         foreach (var user in expiredUsers)
        //         {
        //             _logger.LogInformation("Expiring premium status for user {UserEmail}. Expired at: {ExpiredAt}", 
        //                 user.Email, user.PremiumExpiresAt);
        //             
        //             user.IsPremium = false;
        //             user.SubscriptionStatus = "expired";
        //             await _userRepository.UpdateAsync(user);
        //             expiredCount++;
        //         }
        //         
        //         _logger.LogInformation("Expired premium status for {Count} users", expiredCount);
        //         
        //         return Ok(new { 
        //             message = $"Expired premium status for {expiredCount} users",
        //             expiredCount 
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error checking expired subscriptions");
        //         return StatusCode(500, "Error checking expired subscriptions");
        //     }
        // }
    }
} 