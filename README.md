# 🚗 MecHub - Sistema de Gestão para Oficina Mecânica

> **PIM IV — Análise e Desenvolvimento de Sistemas (UNIP)**

Sistema completo de gestão para oficinas mecânicas, com **API REST**, **aplicativo mobile** e **banco de dados com automação via triggers e procedures**.

---

## 📌 Sobre o projeto

O **MecHub** é um sistema desenvolvido em **ASP.NET Core** que evoluiu do PIM III (site MVC monolítico) para uma **arquitetura distribuída**, permitindo o controle de:

- 👥 Usuários do sistema
- 🧑 Clientes
- 🚗 Veículos
- 🔧 Mecânicos
- 🛠️ Serviços
- 📋 Ordens de serviço
- 📦 Itens vinculados às ordens de serviço
- 🔐 Autenticação local e externa (Google)
- 📱 Aplicativo mobile (mecânico)
- ☁️ Deploy em nuvem (PaaS)

O projeto simula um **ambiente real de oficina**, aplicando conceitos de:

- Desenvolvimento backend
- Arquitetura MVC + API REST
- Autenticação JWT (multi-tenant)
- Relacionamento entre entidades no banco
- **Triggers e Procedures MySQL**
- Cloud e DevOps (Railway)

---

## 🎯 Objetivos do projeto

Este sistema foi desenvolvido com foco em:

- Praticar desenvolvimento profissional com **ASP.NET Core MVC**
- Criar uma **API REST** consumível por aplicações externas
- Implementar **autenticação híbrida** (Cookie + Google + JWT)
- Trabalhar com **Entity Framework Core** + MySQL
- Modelar relacionamentos reais entre entidades
- **Isolamento multi-tenant** (cada mecânico vê só os próprios dados)
- Desenvolver app mobile com **React Native + Expo**
- Aplicar **SQL avançado** (triggers, procedures, tabela de auditoria)
- Preparar uma base escalável para futuras melhorias

---

## 🚀 Tecnologias Utilizadas

### 🧠 Backend

![.NET](https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white)
![C#](https://img.shields.io/badge/CSharp-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET_Core_MVC-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/Entity_Framework_Core-68217A?style=for-the-badge&logo=.net&logoColor=white)
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)

### 📱 Mobile

![React Native](https://img.shields.io/badge/React_Native-20232A?style=for-the-badge&logo=react&logoColor=61DAFB)
![Expo](https://img.shields.io/badge/Expo-000020?style=for-the-badge&logo=expo&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=for-the-badge&logo=typescript&logoColor=white)
![Axios](https://img.shields.io/badge/Axios-5A29E4?style=for-the-badge&logo=axios&logoColor=white)

### 🎨 Front-end (Site)

![Razor](https://img.shields.io/badge/Razor-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=for-the-badge&logo=html5&logoColor=white)
![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=for-the-badge&logo=css3&logoColor=white)
![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=for-the-badge&logo=javascript&logoColor=black)
![Bootstrap](https://img.shields.io/badge/Bootstrap-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)

### 🗄️ Banco de Dados

![MySQL](https://img.shields.io/badge/MySQL-00758F?style=for-the-badge&logo=mysql&logoColor=white)
![MariaDB](https://img.shields.io/badge/MariaDB-003545?style=for-the-badge&logo=mariadb&logoColor=white)
![Pomelo](https://img.shields.io/badge/Pomelo-6DB33F?style=for-the-badge&logo=mysql&logoColor=white)

### 🔧 Ferramentas

![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)
![Git](https://img.shields.io/badge/Git-F05032?style=for-the-badge&logo=git&logoColor=white)
![VS Code](https://img.shields.io/badge/VS_Code-007ACC?style=for-the-badge&logo=visual-studio-code&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)

### 🔐 Autenticação

![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)
![Auth Cookies](https://img.shields.io/badge/Auth-Cookies-orange?style=for-the-badge)
![Google](https://img.shields.io/badge/Google_Auth-4285F4?style=for-the-badge&logo=google&logoColor=white)

### 🏛️ Arquitetura

![MVC](https://img.shields.io/badge/Architecture-MVC-blue?style=for-the-badge)
![REST](https://img.shields.io/badge/API-REST-green?style=for-the-badge)
![Multi-tenant](https://img.shields.io/badge/Multi--tenant-Ativo-orange?style=for-the-badge)

---

## 🏗️ Estrutura do projeto (API)

```bash
MecHub/
│
├── Controllers/         # Controllers MVC (site) + API REST
├── Models/              # Entidades do banco
├── ViewModels/          # Modelos de visualização (MVC)
├── Views/               # Views Razor (site)
├── Data/                # AppDbContext
├── Services/            # TokenService, EmailService, PDF
├── Migrations/          # Migrations do EF Core
├── wwwroot/             # Arquivos estáticos
└── Program.cs
```

---

## 📱 Estrutura do app mobile

```bash
MechubApp/
├── src/
│   ├── app/             # Rotas (telas)
│   ├── contexts/        # AuthContext (token + usuário)
│   ├── services/        # api.ts (Axios + interceptor)
│   ├── constants/       # colors.ts (paleta)
│   └── components/
└── package.json
```

---

## 🎯 Funcionalidades da API

### 🔐 Autenticação

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/AuthApi/login` | Login (devolve JWT com `MecanicoId`) |

### 📋 Ordens de Serviço

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/OrdensServicoApi` | Lista as ordens do mecânico logado |
| GET | `/api/OrdensServicoApi/{id}` | Detalhes de uma OS |
| GET | `/api/OrdensServicoApi/status/{status}` | Filtra por status |
| GET | `/api/OrdensServicoApi/veiculo/{placa}` | Busca por placa |
| GET | `/api/OrdensServicoApi/resumo` | Totais (dashboard) |
| POST | `/api/OrdensServicoApi` | Cria nova OS |
| PUT | `/api/OrdensServicoApi/{id}/status` | Atualiza o status |

### 🧑 Clientes / 🚗 Veículos / 🛠️ Serviços

CRUD completo via API REST.

### 🔧 Mecânicos

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/MecanicosApi/me` | Perfil |
| PUT | `/api/MecanicosApi/me` | Editar perfil |

---

## 🗄️ SQL Avançado (Triggers e Procedures)

### 📋 Tabela de auditoria

```sql
CREATE TABLE log_ordem_servico (
    id INT AUTO_INCREMENT PRIMARY KEY,
    ordem_id INT NOT NULL,
    status_anterior INT,
    status_novo INT,
    data_alteracao DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (ordem_id) REFERENCES ordem_servico(id) ON DELETE CASCADE
);
```

### ⚡ Trigger 1 — Log de mudança de status

```sql
CREATE TRIGGER trg_log_status_os
AFTER UPDATE ON ordem_servico
FOR EACH ROW
INSERT INTO log_ordem_servico (ordem_id, status_anterior, status_novo)
SELECT NEW.id, OLD.status_ordem, NEW.status_ordem
WHERE OLD.status_ordem != NEW.status_ordem;
```

### ⚡ Trigger 2 — Data de fechamento automática

```sql
CREATE TRIGGER trg_data_fechamento
BEFORE UPDATE ON ordem_servico
FOR EACH ROW
SET NEW.data_fechamento = IF(NEW.status_ordem = 4, NOW(), NULL);
```

### 🔧 Procedure — Encerrar OS

```sql
CREATE PROCEDURE encerrar_os(IN p_ordem_id INT)
UPDATE ordem_servico
SET status_ordem = 4, data_fechamento = NOW()
WHERE id = p_ordem_id;
```

---

## 🔐 Segurança

- ✅ JWT com expiração de 8h
- ✅ Multi-tenant (cada mecânico vê só os próprios dados)
- ✅ Senhas com hash (ASP.NET Identity)
- ✅ HTTPS em produção
- ✅ CORS configurado para o mobile

---

## 🚀 Como rodar o projeto

### 🖥️ Backend (API)

```bash
git clone https://github.com/HenriqueBertacchi/mechub-api.git
cd mechub-api
dotnet restore
dotnet run
```

A API estará em `http://localhost:8080` e o Swagger em `http://localhost:8080/swagger`.

> ⚠️ Antes de rodar, configure a string de conexão do MySQL, a chave do JWT e as credenciais do Google (via User Secrets ou variáveis de ambiente). Essas informações **não** ficam no repositório.

### 📱 Mobile (App)

```bash
git clone https://github.com/HenriqueBertacchi/mechub-app.git
cd mechub-app
npm install
npx expo start
```

> Em `src/services/api.ts`, ajuste a URL base para o endereço da sua API.

---

## 📌 Status do projeto

🚧 **Em desenvolvimento** — PIM IV (evolução do PIM III, site MVC monolítico, para API REST + app mobile).

---

## 👨‍💻 Autor



---

⭐ Projeto acadêmico desenvolvido para o curso de Análise e Desenvolvimento de Sistemas — UNIP.