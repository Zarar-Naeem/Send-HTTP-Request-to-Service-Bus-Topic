# Send-HTTP-Request-to-Service-Bus-Topic
It is an Azure Function that will receive an HTTP Request. After receiving the request it will send the complete request to a Service Bus Topic.
It is a secure function as Azure Active Directory is enabled on this function and only those resources can post data who have the credentials for posting data to this Azure Function.
Any type of source can use this Azure function and post data just like a API.
