namespace Ivy.Sliplane
{
    public class SliplaneServiceOptions
    {
        /// <summary>
        /// API Token for authentication (legacy method)
        /// </summary>
        public string? ApiToken { get; set; }
        
        /// <summary>
        /// Organization ID (required when using API Token)
        /// </summary>
        public string? OrganizationId { get; set; }
        
        /// <summary>
        /// OAuth Access Token (alternative to ApiToken)
        /// </summary>
        public string? AccessToken { get; set; }
        
        /// <summary>
        /// Function to retrieve the current access token (for OAuth token refresh scenarios)
        /// </summary>
        public Func<Task<string>>? GetAccessTokenAsync { get; set; }
    }
}
