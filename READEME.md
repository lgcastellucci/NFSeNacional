# 🇧🇷 NFSe-Nacional-ValidadorEnvio

**Validador e Kit de Teste para Integração com a Nota Fiscal de Serviço Eletrônica (NFS-e) de Padrão Nacional.**

Este repositório contém uma solução em C\# para gerar, assinar, comprimir (GZip) e enviar a Declaração de Prestação de Serviços (DPS) para a API da Sefin Nacional, utilizando o layout **Padrão Nacional 1.01**.

O projeto foi desenvolvido com foco na superação de validações XSD complexas e regras de negócio específicas do ambiente de **Produção Restrita (Homologação)**.

## 🌟 Destaques

  * **Validação XSD:** Implementação de regras rigorosas de serialização XML para garantir a conformidade com o Schema do Padrão Nacional.
  * **Assinatura Digital:** Funções para assinar digitalmente a DPS usando certificados A1/A3 (necessário para a Sefin Nacional).
  * **Comunicação API:** Envio da requisição `POST` com o payload compactado (GZip) e codificado (Base64), conforme exigido pela Sefin Nacional.
  * **Log Detalhado:** Captura e tratamento de erros de comunicação, XSD e regras de negócio da API.

## ⚙️ Tecnologias e Dependências

  * **Linguagem:** C\# (.NET Framework ou .NET Core/5+)
  * **Serialização:** `System.Xml.Serialization`
  * **Segurança:** Classes para manipulação de Certificados Digitais (`X509Certificate2`)
  * **Compressão:** Biblioteca para compressão GZip.

## 🚀 Guia Rápido de Uso

### 1\. Pré-requisitos

1.  **Certificado Digital A1/A3:** Necessário para assinar a DPS.
2.  **Ambiente de Homologação:** Acesso e credenciais (se necessário) para a **Produção Restrita** da Sefin Nacional.
3.  **CNPJ Ativo:** O CNPJ do prestador deve estar cadastrado/sincronizado no ambiente de homologação para evitar o erro `E0160`.

### 2\. Configuração

1.  **URL da API:** Configure o endpoint da API de envio da DPS no seu código.

    > **URL de Exemplo (Homologação):** `[INSERIR URL DE HOMOLOGAÇÃO AQUI]`

2.  **Certificado:** Carregue o arquivo `.pfx` ou utilize o certificado instalado na máquina.

    ```csharp
    // Exemplo de carregamento de certificado
    X509Certificate2 certificado = new X509Certificate2("caminho/para/seu/certificado.pfx", "sua_senha");
    ```

3.  **XML:** Crie o objeto da DPS com os dados do Prestador, Tomador e Serviço.

### 3\. Sequência de Processamento

O processo de envio segue os seguintes passos obrigatórios:

1.  **Geração do XML:** Crie o XML da DPS com o namespace `xmlns="http://www.sped.fazenda.gov.br/nfse"`.
2.  **Assinatura Digital:** Assine a tag `<infDPS>` usando o certificado.
3.  **Serialização:** Converta o XML assinado em um array de bytes.
4.  **Compressão (GZip):** Comprima o array de bytes (XML) usando o algoritmo GZip.
5.  **Codificação (Base64):** Converta o resultado da compressão para Base64 (string).
6.  **Envio:** Envie o Base64 em uma requisição `POST` para a API da Sefin Nacional.

## 🧩 Soluções para Erros Comuns

Durante o desenvolvimento, foram identificados e superados os seguintes desafios específicos do ambiente:

| Código de Erro | Descrição Padrão | Causa/Solução no Ambiente de Homologação |
| :--- | :--- | :--- |
| **`E0160`** | Situação do Simples Nacional não confere. | O CNPJ não está sincronizado. **Solução:** O valor `opSimpNac` pode estar **invertido** na validação do servidor (`1` para Não Optante). |
| **`RNG6110`** | Falha Schema Xml (Pattern constraint failed). | Atributo `versao` incorreto. **Solução:** Tentar versões como `101` ou `200` (sem ponto decimal), pois o validador rejeita `1.00`. |
| **`E0617`** | Não é permitido informar alíquota em Simples Nacional. | O XML deve omitir a tag `<tribMun>` e `<pAliq>` se o regime for Simples Nacional ou se o servidor o interpretar como tal. |
| **Ordem de Tags** | Elemento 'X' inválido. Lista de possíveis elementos esperados: 'Y'. | Rigidez extrema na ordem das tags XSD. **Solução:** Garanta que `<tribMun>` venha antes de `<tribFed>` dentro de `<trib>`. |
| **Caracteres Extras** | Elemento 'infDPS' não pode conter texto. | Presença de espaços em branco ou ponto final (`.`) indesejado entre tags XML (Ex: após `<cLocEmi>`). |

## 🤝 Contribuições

Contribuições são bem-vindas\! Se você encontrou alguma inconsistência adicional no ambiente de homologação ou tem melhorias para o processo de envio, por favor, abra uma *Issue* ou envie um *Pull Request*.

## 📜 Licença

Este projeto está sob a licença [MIT / Apache / Sua Licença].