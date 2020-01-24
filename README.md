# devatcli

The swiss army knife for my dotnet core development 

## rabbitmq

Example for publishing and subscribing to a message bus

``` sh
dotnet devatcli.dll rabbitmq -s rabbitmq-0.rabbitmq-headless.rabbitmq.svc.cluster.local -p 5672 -v lab -u <user> -pw <password> subscribe -q labMessageBus

dotnet devatcli.dll rabbitmq -s rabbitmq-0.rabbitmq-headless.rabbitmq.svc.cluster.local -p 5672 -v lab -u <user> -pw <password> publish -q labMessageBus -m Hello


```

## Build And Publish the Docker Image

``` sh
cd src
docker build devatcli -t athurner/devatcli:latest -t athurner/devatcli:1.0.0
docker push athurner/devatcli:latest
docker push athurner/devatcli:1.0.0
```
