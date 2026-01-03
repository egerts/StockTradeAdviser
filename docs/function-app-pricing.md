# Azure Function App Pricing - Y1 (Consumption) Plan

## Overview
The Y1 plan is Azure Functions' serverless consumption plan that provides automatic scaling and pay-per-use pricing.

## Pricing Structure
- **Per-second billing**: You're charged for the exact time your functions run (rounded up to the nearest 100ms)
- **Free grant**: 400,000 GB-seconds of execution time per month (free tier)
- **Memory grant**: 1 GB of memory allocation included in the free grant

## Cost Calculation
**Formula**: (Execution Time in seconds × Memory in GB) - Free Grant

### Example Scenarios:
1. **Small function** (128MB memory, 1 second execution):
   - Cost: 0.128 GB-seconds per execution
   - Free tier covers ~3,125,000 executions/month

2. **Medium function** (512MB memory, 5 seconds execution):
   - Cost: 2.56 GB-seconds per execution
   - Free tier covers ~156,250 executions/month

## Additional Costs
- **Executions**: $0.20 per million executions (after first million free)
- **Storage**: Standard Azure Storage rates for function app files
- **Networking**: Data transfer fees apply

## Benefits for StockTradeAdviser
✅ **Cost-effective**: Only pay when functions are running
✅ **Auto-scaling**: Automatically scales based on demand
✅ **No maintenance**: No servers to manage
✅ **Free tier**: 400,000 GB-seconds free monthly
✅ **Perfect for batch processing**: Hourly stock data ingestion fits well

## Estimated Monthly Cost
For stock data processing (hourly triggers, ~30 seconds runtime, 256MB memory):
- **Daily**: 24 executions × 0.256 GB-seconds = 6.144 GB-seconds
- **Monthly**: ~184 GB-seconds
- **Cost**: $0 (well within free tier)

## Conclusion
The Y1 plan is ideal for the StockTradeAdviser Functions app as it:
- Fits within the free tier for current usage
- Scales automatically if processing needs increase
- Requires no upfront cost or commitment
- Perfect for intermittent workloads like stock data processing
