helm repo add grafana https://grafana.github.io/helm-charts

helm install grafana grafana/grafana -f values.yaml -n monitoring
helm uninstall grafana -n monitoring
kubectl port-forward -n world-service svc/prometheus-grafana 3000:80

kubectl get secret --namespace monitoring grafana -o jsonpath="{.data.admin-password}" | base64 --decode ; echo
kubectl get secret --namespace monitoring grafana -o jsonpath="{.data.admin-password}" | ForEach-Object { [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($\_)) }
