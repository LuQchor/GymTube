using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using GymTube.API.Repositories;

namespace GymTube.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Samo prijavljeni korisnici mogu pristupiti
    public class PaymentsController : BaseController
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;

        public PaymentsController(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration;
            _userRepository = userRepository;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            
            var domain = _configuration["FrontendDomain"] ?? "http://localhost:3000";

            var options = new SessionCreateOptions
            {
                // CustomerId je opcionalan, ali koristan. Ako korisnik već postoji u Stripeu,
                // sva plaćanja će biti vezana za njega.
                Customer = user.StripeCustomerId, 
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = request.PriceId, // Koristimo PriceId koji šalje frontend
                        Quantity = 1,
                    },
                },
                Mode = "subscription", // Mijenjamo u 'subscription'
                SuccessUrl = $"{domain}/profile?payment_success=true&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/profile?payment_canceled=true",
                // Povezujemo plaćanje s korisnikom
                ClientReferenceId = user.Id.ToString()
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return Ok(new { url = session.Url });
        }

        [HttpGet("subscription")]
        public async Task<IActionResult> GetSubscription()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (string.IsNullOrEmpty(user.StripeSubscriptionId))
            {
                return NotFound("No active subscription found.");
            }

            try
            {
                var subscriptionService = new SubscriptionService();
                var subscription = await subscriptionService.GetAsync(user.StripeSubscriptionId);

                DateTime? currentPeriodStart = subscription.Items.Data[0].CurrentPeriodStart;
                DateTime? currentPeriodEnd = subscription.Items.Data[0].CurrentPeriodEnd;

                return Ok(new
                {
                    id = subscription.Id,
                    status = subscription.Status,
                    currentPeriodStart = currentPeriodStart,
                    currentPeriodEnd = currentPeriodEnd,
                    planType = subscription.Items.Data.FirstOrDefault()?.Price.Recurring?.Interval,
                    planAmount = subscription.Items.Data.FirstOrDefault()?.Price.UnitAmount,
                    planCurrency = subscription.Items.Data.FirstOrDefault()?.Price.Currency
                });
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
        }

        [HttpPost("cancel-subscription")]
        public async Task<IActionResult> CancelSubscription()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (string.IsNullOrEmpty(user.StripeSubscriptionId))
            {
                return NotFound("No active subscription found.");
            }

            try
            {
                var subscriptionService = new SubscriptionService();
                var subscription = await subscriptionService.CancelAsync(user.StripeSubscriptionId);

                // Ažuriraj korisnika u bazi - ne mijenjamo IsPremium status
                // Webhook će obraditi status kada se pretplata stvarno otkaže
                user.SubscriptionStatus = subscription.Status;
                await _userRepository.UpdateAsync(user);

                return Ok(new { message = "Subscription will be canceled at the end of the current period." });
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
        }

        [HttpGet("payment-history")]
        public async Task<IActionResult> GetPaymentHistory()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Invalid user ID.");
            var user = await _userRepository.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (string.IsNullOrEmpty(user.StripeCustomerId))
            {
                return Ok(new List<object>()); // Prazna lista ako nema customer ID
            }

            try
            {
                var paymentIntentService = new PaymentIntentService();
                var options = new PaymentIntentListOptions
                {
                    Customer = user.StripeCustomerId,
                    Limit = 50 // Dohvati zadnjih 50 plaćanja
                };

                var paymentIntents = await paymentIntentService.ListAsync(options);

                var payments = paymentIntents.Data.Select(pi => new
                {
                    id = pi.Id,
                    amount = pi.Amount,
                    currency = pi.Currency,
                    status = pi.Status,
                    created = pi.Created,
                    description = pi.Description
                }).ToList();

                return Ok(payments);
            }
            catch (StripeException e)
            {
                return BadRequest(new { error = e.Message });
            }
        }
    }
    
    public class CreateCheckoutSessionRequest
    {
        public string PriceId { get; set; } = string.Empty;
    }
} 