using Microsoft.Extensions.Options;
using nopCommerceApiHub.ApiClient.Token;
using nopCommerceApiHub.WebApi;
using nopCommerceApiHub.WebApi.Services;

namespace PersonalWebApi.Services.NopCommerce
{
    /// <summary>
    /// All objects from Subiekt GT and Nopcommerce
    /// </summary>
    public class NopCommerce
    {
        #region NopCommerce

        private readonly IToken _tokenPage;
        private readonly IOptions<StolargoPLApiSettings> _stolargoPLApiSettingsOptions;
        private readonly IOptionsMonitor<StolargoPLTokentSettings> _stolargoPLTokentSettings;

        public ProductNopCommerceService Product { get; set; }
        public ManufacturerNopCommerceService Manufacturer { get; set; }
        public ProductManufaturerMappingNopCommerceService ProductManufaturerMapping { get; set; }
        public DeliveryDateNopCommerceService DeliveryDate { get; set; }

        #endregion

        public NopCommerce(
            IOptions<StolargoPLApiSettings> stolargoPLApiSettings,
            IOptionsMonitor<StolargoPLTokentSettings> stolargoPLTokentSettings,
            ILogger<NopCommerce> logger
            )
        {

            _stolargoPLApiSettingsOptions = stolargoPLApiSettings;
            _stolargoPLTokentSettings = stolargoPLTokentSettings;
            _tokenPage = getPageToken();

            NopCommerceStolargoPLService nopCommerceStolargoPLService = new NopCommerceStolargoPLService(_stolargoPLApiSettingsOptions.Value.Url, _stolargoPLApiSettingsOptions.Value.TokenEndPoint, _tokenPage, logger);

            Product = new ProductNopCommerceService(nopCommerceStolargoPLService);
            Manufacturer = new ManufacturerNopCommerceService(nopCommerceStolargoPLService);
            ProductManufaturerMapping = new ProductManufaturerMappingNopCommerceService(nopCommerceStolargoPLService);
            DeliveryDate = new DeliveryDateNopCommerceService(nopCommerceStolargoPLService);
        }

        private IToken getPageToken()
        {
            var tokenSettingsManager = new TokenSettingsManager(_stolargoPLTokentSettings);

            var token = new Token(
                _stolargoPLApiSettingsOptions.Value.Username,
                _stolargoPLApiSettingsOptions.Value.Password,
                tokenSettingsManager,
                "NopCommerceStolargoPLTokenSettings");

            return token;
        }

        // check connection to api
        public async Task<bool> CheckConnectionAsync()
        {
            try
            {
                await DeliveryDate.GetAllAsync();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
    }
}
