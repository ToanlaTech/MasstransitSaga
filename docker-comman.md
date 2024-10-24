docker build --no-cache -f MasstransitSaga.OrderAcceptService/Dockerfile -t order.accept:v.0.1 .
docker build --no-cache -f MasstransitSaga.OrderCompleteService/Dockerfile -t order.complete:v.0.1 .
docker build --no-cache -f MasstransitSaga.OrderSubmitService/Dockerfile -t order.submit:v.0.1 .
docker build --no-cache -f MasstransitReactApp.Server/Dockerfile -t order.server:v.0.1 .