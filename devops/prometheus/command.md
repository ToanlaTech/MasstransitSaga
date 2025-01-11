helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update

helm install prometheus prometheus-community/kube-prometheus-stack -f values.yaml --namespace world-service
helm upgrade prometheus prometheus-community/kube-prometheus-stack -f values.yaml --namespace world-service
helm upgrade --install prometheus prometheus-community/kube-prometheus-stack -f values.yaml -n world-service

helm uninstall prometheus -n world-service
kubectl port-forward svc/prometheus-operated -n world-service 9090:9090
kubectl get podmonitor -n world-service
kubectl get servicemonitor -n world-service

kubectl get secret prometheus-grafana -n world-service -o jsonpath="{.data.admin-password}" | base64 --decode

//Lệnh port-forward giúp bạn chuyển tiếp cổng từ máy cục bộ đến Service Grafana bên trong Kubernetes.
kubectl port-forward -n world-service svc/prometheus-grafana 3000:80
