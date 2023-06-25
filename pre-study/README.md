This is the documentation of the pre-studi for the final project.

## Goal

We aim to create another deployment configuration besides our `docker-compose` configuration. 

For this we want to move our deployment to a kubernetes cluster. We will use minikube for this.

We want to create a kubernetes deployment that would allow us to deploy our services and monitor them as simple as possible.

For this we want to use helm to deploy our services and istio as a service mesh to add monitoring capabilities ot our services without changing any code.

## Helm

Helm is a package manager for kubernetes. It allows us to deploy our services with a single command and easily add other services anythime we want.

Helm works with charts. A chart is a collection of files that describe a related set of kubernetes resources. Deploying a chart also deploys all the resources and dependencies described in the chart.

If we want to deploy our services with helm, we need to create a chart for each service. Some services have dependencies on other services, so they will need to have them as subcharts inside their chart. This is achieved, by putting the chart of a dependency in the `charts` directory of the chart that depends on it.

This leads our directory structure to look like this:

```
all-services
    ├── charts
    │   ├── auth-service
    │   │   ├── Chart.yaml
    │   │   ├── templates
    │   │   └── values.yaml
    │   ├── product-management
    │   │   ├── Chart.yaml
    │   │   ├── templates
    │   │   └── values.yaml
    │   └── product-service
    │       ├── charts
    │       │   ├── postgresql-12.5.7.tgz
    │       │   └── rabbitmq-12.0.1.tgz
    │       ├── Chart.yaml
    │       ├── templates
    │       └── values.yaml
    ├── Chart.yaml
    ├── templates
    └── values.yaml
```

The all-services chart is the umbrella chart that deploys all the services and ensure that all of them have the same database configuration.

But lets look at the charts themselves. They all have the same structure. They have a `Chart.yaml` file, a `templates` directory and a `values.yaml` file. The `Chart.yaml` file contains the metadata for the chart. The `templates` directory contains the templates for the kubernetes resources that will be deployed. The `values.yaml` file contains the default values for the variables that are used in the templates.

The `product-service` chart is the chart for the product service. It has a dependency on the `postgresql` and `rabbitmq` charts (from `bitnami`). These charts deploy a `postgresql` and `rabbitmq` instance respectively. Without them, the `product-service` can not perform its tasls.

On the same level as the `product-service` chart is the `auth-service` chart. It does not have any dependencies, so it can be deployed on its own. Via the `all-services`chart we can ensure that both charts get the same `APP_JWT_SECRET`.

The `product-management`chart deploys our graphql service, it also actually has a dependency on `postgres`but we do not know how we can structure it better than we currently aim to do.

## Istio

Istio is a service mesh. It allows us to monitor our services and perform traffic management. Istio would give us observabillity and tracing out of the box and we should be able to use its gateway to expose our services to the outside world.

We tried looking into whether or not it is possible to add istio as a dependency to our charts, but all info we found pretty much said while possible it is not recommended. We think this might have to do with how istio needs to be installed, there are clear requirements in the order of chart installation if you follow the helm guide and as far as we know, helm is not capable of achieving this when using dependencies. We might be wrong about this, but we did not find any info on how to do this.

So the solution for the project will be to give the instructions on how to set up istio and auto inject the sidecar proxies into the pods. The sidecar proxies are needed for istio to properly monitor the services.

Since we can not setup istio as a dependency, we will also not use the istio gateway to expose our services. We will use the kubernetes ingress instead. This would allow us to still deploy and make our services available with a single helm command and mean that the deplyoment does not need istio to be deployed in order to work. So istio would be an optional feature, that can be added to the deployment if desired.
