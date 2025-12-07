package publisher

import (
	"services/internal/constants"
	router "services/internal/infrastructure/messagerouter"
)

func JobResultExchangeConfig() router.ExchangeConfig {
	return router.ExchangeConfig{
		ExchangeName: constants.JobResultQueue,
		ExchangeType: constants.ExchangeFanout,
		RoutingKey:   constants.RoutingKeyHash,
		Durable:      true,
	}
}
