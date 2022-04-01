# LN Sign - Assinatura Digital
## _Api simples em REST para assinar digitalmente documentos_

Repositório mantido por: [Nymeria Desenvolvimento de Sistemas](https://nymeriasoft.com.br)

LN Sign é um programa que pode ser instalado no windows para que você seja capaz de assinar documentos com certificado A3 cartão pessoal. 
O mais legal é que ele liga um servidor Http localmente com duas rotas disponíveis:
- Listar certificados A3 instalados na maquina local
- Assinar documento PDF

Você pode integrar o LNSign com qualquer tipo de sistema. 

## Instalação

- Faça o download da versão latest [releases](https://github.com/leonetosoft/lnsign/releases/tag/winx86)
- Foi testado no Windows  10 deve funcionar em versões mais atualizadas

**Como listar os certificados**
----
  _Nessa rota é possível listar os certificados instalados localmente_

* **URL:** http://127.0.0.1:8888/api/cert

* **Method:**   `GET`

* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```
    { 
         code: 1,
         msg: ""
         data: [
           {
             subject: "CN=EMPRESA, OU=AR SERASA, OU=NUMERACAO, OU=AC SERASA RFB v5, OU=RFB e-CPF A3, OU=Secretaria da Receita Federal do Brasil - RFB, O=ICP-Brasil, C=BR",
             description: "Descrição do certificado, nome da empresa"
           } ....
          ]
    }
* **Error Response:**
_A api sempre vai retornar o código 200 (código HTTP). Para identificar os erros veja o JSON retornado, ele possui um campo "code" para identificar o erro. Quando este código for 1 não houve nenhum erro caso retornar diferente disso você terá algo assim:_

   ```
    { 
         code: 2,
         msg: "MENSAGEM DE ERRO"
    }
* **Notes:**

  Sempre valide o codigo de erro!

**Assinar documentos PDF**
----
  _Nessa rota é possível listar os certificados instalados localmente_

* **URL:** http://127.0.0.1:8888/api/sign

* **Method:**   `POST`
* **Corpo da solicitação:**
   ```
    {
       "CertSubject":  "Subject do certificado",
       "URInputPdf":  "(Arquivo que você deseja assinar) Ex: C:/assinar.pdf ou https://teste.com/assinar.pdf",
       "URIOutputPdf":  "(Arquivo de saída, o arquivo que foi assinado) Ex: C:/assinado.pdf ou https://teste.com/assinado.pdf",
       "URImgSignature":  "(Imagem que da assinatura PNG ) C:/assinatura.png ou https://teste.com/assinatura.png",
       "ImgSignaturePositionX":  30  (onde vai aparecer a assinatura na pos X do documento), 
       "ImgSignaturePositionY":  150  (onde vai aparecer a assinatura na pos Y do documento), 
       "ImgSignatureWidth":  50, (largura da assinatura),
        "ImgSignatureHeight":  50, (altura da assinatura),
       "SignatureReason":  "teste", (Esse campo aparece no PDF assinado quando abre ele no adobe)
       "SignatureLocation":  "teste"(Esse campo aparece no PDF assinado quando abre ele no adobe)
    } 
* **Success Response:**

  * **Code:** 200 <br />
    **Content:** 
    ```
    { 
         code: 1,
         msg: ""
         data: {
	          base64: "BASE 64 DO ARQUIVO ASSINADO"    
         }
          
    }
* **Error Response:**
_A api sempre vai retornar o código 200 (código HTTP). Para identificar os erros veja o JSON retornado, ele possui um campo "code" para identificar o erro. Quando este código for 1 não houve nenhum erro caso retornar diferente disso você terá algo assim:_

   ```
    { 
         code: 2,
         msg: "MENSAGEM DE ERRO"
    }
## Segurança
Caso queira distribuir o software recomendo que defina as políticas de CORS para aceitar somente requisições de seu sistema específico. 
Também seria legal colocar algum tipo de chave no corpo das requisições.

## License

MIT

**Free Software, Hell Yeah!**
Nos ajude a melhorar este projeto!