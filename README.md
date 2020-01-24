# lab-masstransit
lab masstransit

## Build And Publish the Docker Image

``` sh
cd src
docker build lab-masstransit -t athurner/lab-masstransit:latest -t athurner/lab-masstransit:1.0.0
docker push athurner/lab-masstransit:latest
docker push athurner/lab-masstransit:1.0.1
```
