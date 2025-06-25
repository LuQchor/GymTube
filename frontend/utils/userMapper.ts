export function mapUserApiToFrontend(user: any) {
  return {
    id: user.Id || user.id,
    email: user.Email || user.email,
    name: user.Name || user.name,
    is_premium: user.IsPremium || user.is_premium,
    role: user.Role || user.role,
    profileImageUrl: user.ProfileImageUrl || user.profile_image_url,
    description: user.Description || user.description,
    gender: user.Gender || user.gender,
    birthDate: user.BirthDate || user.birth_date,
    isProfilePrivate: user.IsProfilePrivate || user.is_profile_private,
    premium_expires_at: user.PremiumExpiresAt || user.premium_expires_at,
    hasPassword: user.hasPassword
  }
}

