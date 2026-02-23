# ObservabilityPOC

# 🚀 Subindo o SigNoz com Docker (modo simples)

## ✅ Pré-requisitos

* Docker instalado
* Docker Compose (ou Docker Desktop já resolve)

---

## 1️⃣ Clonar o repositório oficial

```bash
git clone https://github.com/SigNoz/signoz.git
cd signoz/deploy/docker
```

---

## 2️⃣ Subir os containers

```bash
docker-compose up -d
```

Na primeira vez pode demorar alguns minutos porque ele baixa as imagens (ClickHouse, OTEL Collector, etc).

---

## 3️⃣ Acessar no navegador

Depois que subir:

```
http://localhost:3301
```

Pronto. O painel já estará rodando.

---

# 🧠 O que vai subir junto?

O stack padrão sobe:

* UI do SigNoz
* OpenTelemetry Collector
* ClickHouse (banco de métricas e traces)

Tudo containerizado.

---

# 📡 Depois disso

Para monitorar sua API .NET 8, você vai:

1. Instrumentar com OpenTelemetry
2. Apontar o exporter para o OTEL Collector do SigNoz
3. Visualizar traces, métricas e logs na UI

---
