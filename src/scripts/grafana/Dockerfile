FROM grafana/grafana:9.4.3
COPY ./prometheus-connector.yml  /etc/grafana/provisioning/datasources/ 
COPY ./prometheus-dashboards.yml  /etc/grafana/provisioning/dashboards/
COPY ./dashboards/custom-dasboard.json  /var/lib/grafana/dashboards/
COPY ./dashboards/dotnet-performance-counters-dashboard2.json  /var/lib/grafana/dashboards/