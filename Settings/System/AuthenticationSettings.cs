namespace PersonalWebApi.Settings.System
{
    /// <summary>
    /// Settings related to authentication.
    /// </summary>
    public class AuthenticationSettings
    {
        /// <summary>
        /// Gets or sets the JWT key used for signing tokens.
        /// </summary>
        public string JwtKey { get; set; }

        /// <summary>
        /// Gets or sets the number of days until the JWT token expires.
        /// </summary>
        public int JwtExpireDays { get; set; }

        /// <summary>
        /// Gets or sets the issuer of the JWT token.
        /// </summary>
        public string JwtIssuer { get; set; }
    }
}
