#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.
FROM mcr.microsoft.com/mssql/server:2019-latest

RUN mkdir /var/opt/mssql/backup
COPY POSSystemsDBV2.bak /var/opt/mssql/backup/.
COPY entrypoint.sh .
COPY run-initialization.sh .

ENV SA_PASSWORD yourStrong@Password
ENV ACCEPT_EULA Y
ENV MSSQL_PID=Developer
ENV MSSQL_TCP_PORT=1433 

CMD /bin/bash ./entrypoint.sh