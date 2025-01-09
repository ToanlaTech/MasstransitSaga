docker build --no-cache -f MasstransitSaga.OrderAcceptService/Dockerfile -t order.accept:v.0.1 .
docker build --no-cache -f MasstransitSaga.OrderCompleteService/Dockerfile -t order.complete:v.0.1 .
docker build --no-cache -f MasstransitSaga.OrderSubmitService/Dockerfile -t order.submit:v.0.1 .
docker build --no-cache -f MasstransitReactApp.Server/Dockerfile -t order.server:v.0.1 .
docker run -d --name rabbitmq -e RABBITMQ_DEFAULT_USER=admin -e RABBITMQ_DEFAULT_PASS=123456789 -p 5672:5672 -p 15672:15672  rabbitmq:3-management
docker run -d  --name postgresql -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=order -v pgdata:/var/lib/postgresql/data -p 5432:5432  postgres:latest

docker run --name mysql-container -e MYSQL_ROOT_PASSWORD=123456 -e MYSQL_DATABASE=world -e MYSQL_USER=admin -e MYSQL_PASSWORD=123456 -p 3306:3306 -d mysql:8.0

