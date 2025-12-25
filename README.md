# StockTradeAdviser

**An experimental project to learn and showcase development with AI agents (Windsurf and Copilot).**

This is an AI-powered stock trading advisory system that provides personalized buy/sell recommendations based on user-defined trading strategies. The project serves as a practical exploration of how AI coding assistants can collaborate to build a complex, cloud-native application.

## Architecture Overview

StockTradeAdviser is built as a cloud-native application on Azure with the following components:

### Backend Services
- **Azure Functions**: Serverless functions for stock data ingestion and recommendation generation
- **ASP.NET Core API**: RESTful API for frontend communication
- **Azure Cosmos DB**: NoSQL database for storing user data, portfolios, and recommendations
- **Azure Service Bus**: Message queuing for asynchronous processing
- **Azure Event Grid**: Event-driven architecture for real-time updates

### Frontend
- **React 18** with TypeScript for the user interface
- **Azure AD B2C** for authentication and authorization
- **Tailwind CSS** for modern, responsive styling

### Data Sources
- **Alpha Vantage API** for real-time stock market data
- **Technical Analysis Engine** for calculating indicators (RSI, MACD, Bollinger Bands)
- **Fundamental Analysis** for financial metrics evaluation

## Features

### 🎯 Personalized Recommendations
- AI-driven buy/sell/hold recommendations
- Confidence scores and risk assessments
- Target price and stop-loss calculations
- Time horizon predictions

### 📊 Portfolio Management
- Create and manage multiple portfolios
- Track holdings across stocks, options, ETFs, and more
- Real-time profit/loss calculations
- Transaction history tracking

### 📈 Technical & Fundamental Analysis
- Real-time technical indicators (RSI, MACD, SMA, EMA, Bollinger Bands)
- Fundamental metrics (P/E ratio, revenue growth, ROE, debt ratios)
- Market sentiment analysis
- Sector-based recommendations

### 🔔 Smart Alerts
- Price alerts for target/stop-loss levels
- Daily portfolio summaries
- New recommendation notifications

### 🛡️ Security & Compliance
- Azure AD B2C authentication
- Role-based access control
- Data encryption at rest and in transit
- GDPR compliant data handling

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js 18.x or later
- Azure subscription
- Alpha Vantage API key

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/StockTradeAdviser.git
   cd StockTradeAdviser
   ```

2. **Backend Setup**
   ```bash
   # Restore NuGet packages
   dotnet restore
   
   # Set up local configuration
   cp src/StockTradeAdviser.Api/appsettings.json.example src/StockTradeAdviser.Api/appsettings.json
   cp src/StockTradeAdviser.Functions/local.settings.json.example src/StockTradeAdviser.Functions/local.settings.json
   
   # Update configuration files with your API keys and connection strings
   ```

3. **Frontend Setup**
   ```bash
   cd frontend
   npm install
   cp .env.example .env
   # Update .env with your Azure AD configuration
   ```

4. **Run the application**
   ```bash
   # Start the API (in one terminal)
   cd src/StockTradeAdviser.Api
   dotnet run
   
   # Start the Functions (in another terminal)
   cd src/StockTradeAdviser.Functions
   func start
   
   # Start the frontend (in another terminal)
   cd frontend
   npm start
   ```

### Azure Deployment

1. **Infrastructure Deployment**
   ```bash
   # Deploy Azure resources using Bicep
   az deployment group create \
     --resource-group StockTradeAdviser-RG \
     --template-file infrastructure/main.bicep \
     --parameters infrastructure/parameters.json
   ```

2. **Application Deployment**
   ```bash
   # Deploy using GitHub Actions (automatic on push to main)
   # Or deploy manually:
   dotnet publish src/StockTradeAdviser.Api/StockTradeAdviser.Api.csproj -c Release -o ./publish
   az webapp deployment source config-zip --resource-group StockTradeAdviser-RG --name StockTradeAdviser-API --src ./publish.zip
   ```

## Configuration

### Environment Variables

#### Backend (.NET)
```json
{
  "CosmosDb:Endpoint": "https://your-cosmos-account.documents.azure.com:443/",
  "CosmosDb:Key": "your-cosmos-key",
  "CosmosDb:DatabaseName": "StockTradeAdviser",
  "ServiceBus:ConnectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key",
  "AlphaVantage:ApiKey": "your-alpha-vantage-key",
  "AzureAd:Instance": "https://login.microsoftonline.com/",
  "AzureAd:Domain": "your-tenant-domain",
  "AzureAd:TenantId": "your-tenant-id",
  "AzureAd:ClientId": "your-client-id",
  "AzureAd:ClientSecret": "your-client-secret"
}
```

#### Frontend (React)
```bash
REACT_APP_AZURE_AD_CLIENT_ID=your-client-id
REACT_APP_AZURE_AD_TENANT_ID=your-tenant-id
```

## API Documentation

### Authentication
All API endpoints require authentication via Azure AD B2C. Include the access token in the Authorization header:

```
Authorization: Bearer <access-token>
```

### Key Endpoints

#### Portfolio Management
- `GET /api/portfolio` - Get user portfolios
- `POST /api/portfolio` - Create new portfolio
- `PUT /api/portfolio/{id}` - Update portfolio
- `DELETE /api/portfolio/{id}` - Delete portfolio

#### Recommendations
- `GET /api/recommendations` - Get user recommendations
- `GET /api/recommendations/active` - Get active recommendations
- `POST /api/recommendations/generate` - Generate new recommendations
- `PUT /api/recommendations/{id}/execute` - Execute recommendation

#### Stock Data
- `GET /api/stocks/{symbol}` - Get stock data
- `GET /api/stocks/market-overview` - Get market overview

## Monitoring & Logging

### Application Insights
- Performance monitoring
- Error tracking
- User behavior analytics
- Dependency tracking

### Azure Monitor
- Resource utilization metrics
- Custom metrics and alerts
- Log analytics queries

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Code Style
- Follow C# coding conventions for backend
- Use ESLint and Prettier for frontend
- Write unit tests for new features
- Update documentation for API changes

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support and questions:
- Create an issue in the GitHub repository
- Contact the development team
- Check the [documentation](docs/) for detailed guides

## Roadmap

### Upcoming Features
- [ ] Mobile app (React Native)
- [ ] Advanced charting and technical analysis
- [ ] Social trading features
- [ ] Options trading support
- [ ] Machine learning model improvements
- [ ] Real-time streaming data
- [ ] Multi-currency support
- [ ] Tax reporting features

### Performance Improvements
- [ ] Caching layer with Redis
- [ ] Database query optimization
- [ ] CDN implementation
- [ ] Load testing and optimization

## Security Considerations

- Regular security audits
- Dependency vulnerability scanning
- Penetration testing
- Data backup and disaster recovery
- Compliance with financial regulations

**Disclaimer**: This software is for educational and informational purposes only. Stock trading involves substantial risk of loss and is not suitable for all investors. Always do your own research before making investment decisions.
