helm install rabbitmq bitnami/rabbitmq -f values.yaml --namespace world-service
helm upgrade rabbitmq bitnami/rabbitmq --namespace world-service -f values.yaml
kubectl port-forward svc/rabbitmq -n world-service 15672:15672
helm uninstall rabbitmq --namespace world-service
kubectl logs rabbitmq-0 -n world-service
kubectl exec -it rabbitmq-0 -n world-service -- netstat -tlnp
kubectl exec -it rabbitmq-0 -n world-service -- rabbitmq-plugins disable rabbitmq_management
kubectl exec -it rabbitmq-0 -n world-service -- rabbitmq-plugins enable rabbitmq_management

kubectl exec -it rabbitmq-0 -n world-service -- rabbitmq-plugins enable rabbitmq_prometheus

kubectl exec -it rabbitmq-0 -n world-service -- rabbitmq-plugins list
kubectl run -it --rm debug --image=busybox --restart=Never -n world-service -- wget -qO- http://10.10.36.3:9419/metrics
kubectl exec -it rabbitmq-0 -n world-service -- rabbitmq-plugins list
