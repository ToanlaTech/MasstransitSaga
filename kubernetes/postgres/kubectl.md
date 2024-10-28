kubectl apply -f patroni-config.yaml
kubectl apply -f etcd-deployment.yaml
kubectl apply -f patroni-rbac.yaml
kubectl apply -f patroni-statefulset.yaml
Sử dụng lệnh sau để xác định địa chỉ IP của node hoặc cluster mà bạn sẽ kết nối từ bên ngoài:
kubectl get nodes -o wide