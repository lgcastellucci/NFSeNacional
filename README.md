# 🇧🇷 NFSe Nacional
Utilização das APIs NFSe-Nacional

**Agradecimentos ao fork inicial https://github.com/WanderleyCoellho/NFSe-Nacional-ValidadorEnvio**


Este repositório contém uma solução em C\#.net10 para contsultar uma NFSe gerada no layout **Padrão Nacional 1.01** como resultado seu PDF e XML

## ⚙️ Tecnologias e Dependências

  * **Linguagem:** C\# (.NET 10)
  * **Serialização:** `System.Xml.Serialization`
  * **Segurança:** Classes para manipulação de Certificados Digitais (`X509Certificate2`)
  * **Compressão:** Biblioteca para compressão GZip.

## 🚀 Guia Rápido de Uso

### 1\. Pré-requisitos

1.  **Certificado Digital A1/A3:** Necessário para assinar a DPS.
2.  **Ambiente de Produção:** Acesso ao certificado digital (formato PEM) do prestador.

### 2\. Configuração

1.  **Certificado:** Carregue o arquivo `.pem` via parametro de inicialização da aplicação.
2.  **Chave:** Carregue a chave da NFSe via parametro de inicialização da aplicação.

### 3\. Sequência de Processamento

1.  **Geração do PDF:** Criar o PDF.
2.  **Geração do XML:** Criar o XML.

## 🧩 Soluções para Erros Comuns

## 🤝 Contribuições

Contribuições são bem-vindas\! Se você encontrou alguma inconsistência adicional no ambiente de homologação ou tem melhorias para o processo de envio, por favor, abra uma *Issue* ou envie um *Pull Request*.

## 📜 Licença

Este projeto está sob a [Licença MIT](LICENSE).
