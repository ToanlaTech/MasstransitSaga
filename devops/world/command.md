kubectl get namespaces
kubectl create namespace my-namespace

helm repo add bitnami https://charts.bitnami.com/bitnami
helm install mysql-db bitnami/mysql -f values.yaml --namespace world-service
helm uninstall mysql-db --namespace world-service

kubectl get pvc --namespace world-service
kubectl describe pvc data-mysql-db-0 --namespace world-service

kubectl get storageclass
kubectl get secret --namespace world-service mysql-db -o jsonpath="{.data.mysql-root-password}" | base64 --decode; echo

kubectl run -it --rm debug --image=busybox --restart=Never -n world-service -- wget -qO- http://10.1.0.44:8083/metrics

kubectl apply -f deployment.yml
kubectl apply -f service.yml
kubectl apply -f world-api-servicemonitor.yaml