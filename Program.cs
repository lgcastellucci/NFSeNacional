using NFSeNacional.Services;
using System.Drawing;
using System.Text;
using System.Text.Json;

namespace NFSeNacional
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {

                    Console.WriteLine("Uso EnvioNacional: NFSeNacional EN <caminho_do_certificado.pem> <dados_envio.json>");
                    Console.WriteLine("Uso ConsultaNacional: NFSeNacional CN <caminho_do_certificado.pem> <chave_da_nf>");
                    Console.WriteLine("Uso EnvioGinfes: NFSeNacional EG <caminho_do_certificado.pem> <dados_envio.json>");
                    Thread.Sleep(5000);

                    // NFSeNacional CN OPERADORA_002.pem 35387092203966317000175000000000001726010489905886

                    return;
                }

                string tipoOperacao = args[0].ToUpper(); // EN = Envio Nacional, CN = Consulta Nacional, EG = Envio Ginfes
                string caminhoCertificado = args[1];
                string chave = args[2];

                tipoOperacao = "EG";
                caminhoCertificado = "OPERADORA_008.pem";
                chave = "DadosNFSe_008.json";

                LogService.Log("Caminho certificado: " + caminhoCertificado, Color.Blue);
                LogService.Log("NFSe Chave: " + chave, Color.Blue);


                if (tipoOperacao == "EN")
                {
                    #region DPD = Declaração de Prestação de Serviço
                    if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chave)))
                    {
                        LogService.Log("Arquivo json não encontrado.", Color.Red);
                        return;
                    }

                    var dadosString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chave));
                    var jsonDadosNFSe = JsonDocument.Parse(dadosString);

                    var rpsNumero = jsonDadosNFSe.RootElement.GetProperty("RpsNumero").GetString();
                    var rpsSerie = jsonDadosNFSe.RootElement.GetProperty("RpsSerie").GetString();

                    var prestadorCpfCnpj = jsonDadosNFSe.RootElement.GetProperty("Prestador").GetProperty("CpfCnpj").GetString();
                    var prestadorInscricaoMunicipal = jsonDadosNFSe.RootElement.GetProperty("Prestador").GetProperty("InscricaoMunicipal").GetString();
                    var prestadorMunicipio = jsonDadosNFSe.RootElement.GetProperty("Prestador").GetProperty("Municipio").GetString();

                    var tomadorCpfCnpj = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("CpfCnpj").GetString();
                    var tomadorRazaoSocial = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("RazaoSocial").GetString();
                    var tomadorMunicipio = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Municipio").GetString();
                    var tomadorCep = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Cep").GetString();
                    var tomadorLogradouro = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Logradouro").GetString();
                    var tomadorNumero = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Numero").GetString();
                    var tomadorBairro = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Bairro").GetString();
                    var tomadorEmail = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Email").GetString();

                    var codigoTributacao = jsonDadosNFSe.RootElement.GetProperty("Servico").GetProperty("CodigoTributacao").GetString();
                    var descricaoServico = jsonDadosNFSe.RootElement.GetProperty("Servico").GetProperty("Descricao").GetString();
                    var informacaoComplementar = jsonDadosNFSe.RootElement.GetProperty("Servico").GetProperty("InformacaoComplementar").GetString();
                    var valorServico = jsonDadosNFSe.RootElement.GetProperty("Servico").GetProperty("Valor").GetDecimal().ToString("F2").Replace(",", ".");

                    var dataEmissao = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
                    var dataCompetencia = DateTime.Now.ToString("yyyy-MM-dd");


                    //id = DPS + CNPJ do emitente + Código do Município + Série + Número da DPS
                    var idDps = "DPS" + prestadorMunicipio.PadLeft(7, '0') + "2" + prestadorCpfCnpj.PadLeft(14, '0') + rpsSerie.PadLeft(5, '0') + rpsNumero.PadLeft(15, '0');

                    string xmlParaEnviar = string.Empty;

                    #region 1. Criar XML Bruto
                    var xmlDPS = new StringBuilder();
                    xmlDPS.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    xmlDPS.Append("<DPS xmlns=\"http://www.sped.fazenda.gov.br/nfse\" versao=\"1.00\">");
                    xmlDPS.Append("  <infDPS Id=\"" + idDps + "\">");
                    xmlDPS.Append("    <tpAmb>1</tpAmb>"); // 1 - Produção, 2 - Homologação
                    xmlDPS.Append("    <dhEmi>" + dataEmissao + "</dhEmi>");
                    xmlDPS.Append("    <verAplic>ACBrNFSeX-1.00</verAplic>"); // Versão do sistema emissor
                    xmlDPS.Append("    <serie>" + rpsSerie + "</serie>");
                    xmlDPS.Append("    <nDPS>" + rpsNumero + "</nDPS>");
                    xmlDPS.Append("    <dCompet>" + dataCompetencia + "</dCompet>");
                    xmlDPS.Append("    <tpEmit>1</tpEmit>"); // 1 - Emissão pelo próprio prestador
                    xmlDPS.Append("    <cLocEmi>" + prestadorMunicipio + "</cLocEmi>");
                    xmlDPS.Append("    <prest>");
                    xmlDPS.Append("      <CNPJ>" + prestadorCpfCnpj + "</CNPJ>");
                    xmlDPS.Append("      <IM>" + prestadorInscricaoMunicipal + "</IM>");
                    xmlDPS.Append("      <regTrib>");
                    xmlDPS.Append("        <opSimpNac>1</opSimpNac>"); // 1 - Optante Simples Nacional
                    xmlDPS.Append("        <regEspTrib>0</regEspTrib>"); // 0 - Não se enquadra em nenhuma das opções
                    xmlDPS.Append("      </regTrib>");
                    xmlDPS.Append("    </prest>");
                    xmlDPS.Append("    <toma>");
                    xmlDPS.Append("      <CNPJ>" + tomadorCpfCnpj + "</CNPJ>");
                    xmlDPS.Append("      <xNome>" + tomadorRazaoSocial + "</xNome>");
                    xmlDPS.Append("      <end>");
                    xmlDPS.Append("        <endNac>");
                    xmlDPS.Append("          <cMun>" + tomadorMunicipio + "</cMun>");
                    xmlDPS.Append("          <CEP>" + tomadorCep + "</CEP>");
                    xmlDPS.Append("        </endNac>");
                    xmlDPS.Append("        <xLgr>" + tomadorLogradouro + "</xLgr>");
                    xmlDPS.Append("        <nro>" + tomadorNumero + "</nro>");
                    xmlDPS.Append("        <xBairro>" + tomadorBairro + "</xBairro>");
                    xmlDPS.Append("      </end>");
                    if (!string.IsNullOrEmpty(tomadorEmail))
                        xmlDPS.Append("      <email>" + tomadorEmail + "</email>");
                    xmlDPS.Append("    </toma>");
                    xmlDPS.Append("    <serv>");
                    xmlDPS.Append("      <locPrest>");
                    xmlDPS.Append("        <cLocPrestacao>" + prestadorMunicipio + "</cLocPrestacao>");
                    xmlDPS.Append("      </locPrest>");
                    xmlDPS.Append("      <cServ>");
                    xmlDPS.Append("        <cTribNac>" + codigoTributacao + "</cTribNac>");
                    xmlDPS.Append("        <xDescServ>" + descricaoServico + "</xDescServ>");
                    xmlDPS.Append("      </cServ>");
                    if (!string.IsNullOrEmpty(informacaoComplementar))
                    {
                        xmlDPS.Append("      <infoCompl>");
                        xmlDPS.Append("        <xInfComp>" + informacaoComplementar + "</xInfComp>");
                        xmlDPS.Append("      </infoCompl>");
                    }
                    xmlDPS.Append("    </serv>");
                    xmlDPS.Append("    <valores>");
                    xmlDPS.Append("      <vServPrest>");
                    xmlDPS.Append("        <vServ>" + valorServico + "</vServ>");
                    xmlDPS.Append("      </vServPrest>");
                    xmlDPS.Append("      <trib>");
                    xmlDPS.Append("        <tribMun>");
                    xmlDPS.Append("          <tribISSQN>1</tribISSQN>"); // 1 - ISSQN devido no município
                    xmlDPS.Append("          <tpRetISSQN>1</tpRetISSQN>"); // 1 - Não Retido
                    xmlDPS.Append("        </tribMun>");
                    xmlDPS.Append("        <totTrib>");
                    xmlDPS.Append("          <indTotTrib>0</indTotTrib>"); // 0 - Não compõe o total da nota fiscal
                    xmlDPS.Append("        </totTrib>");
                    xmlDPS.Append("      </trib>");
                    xmlDPS.Append("    </valores>");
                    xmlDPS.Append("  </infDPS>");
                    xmlDPS.Append("</DPS>");

                    xmlParaEnviar = xmlDPS.ToString();
                    #endregion

                    #region 2. Assinatura Digital (Se tiver serial)
                    LogService.Log("Assinando o XML digitalmente...", Color.Blue);
                    var assinador = new AssinadorDigital(caminhoCertificado);

                    // Substitui o XML bruto pelo XML Assinado
                    xmlParaEnviar = assinador.AssinarDps(xmlParaEnviar);
                    LogService.Log("XML Assinado com sucesso!", Color.Green);
                    #endregion

                    #region 3. Validação XSD (Agora validamos o XML já assinado ou o original)
                    var pathXSD = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XSD");
                    LogService.Log("Validando Schema XSD...");
                    var validador = new NfseValidator(NfseValidator.Provedor.Nacional, pathXSD);

                    if (validador.ValidarXml(xmlParaEnviar))
                    {
                        // 4. Envio para API
                        LogService.Log("Enviando para API Nacional...", Color.Blue);
                        var servicoEnvio = new NfseService(caminhoCertificado);

                        var resposta = servicoEnvio.EnviarDps(xmlParaEnviar);
                        if (resposta.sucesso)
                            LogService.Log("Chave de Acesso: " + resposta.chaveAcesso, Color.Green);
                        else
                            LogService.Log("FALHA NO ENVIO:" + resposta.mensagem, Color.Red);

                    }
                    else
                    {
                        LogService.Log("XML INVÁLIDO:", Color.Red);
                        foreach (var erro in validador.Erros)
                            LogService.Log(erro, Color.Red);
                    }
                    #endregion

                    #endregion
                }
                else if (tipoOperacao == "CN")
                {
                    var servico = new NfseService(caminhoCertificado);

                    #region Consulta da NFSe (XML)
                    var respostaXml = servico.ConsultarNFSe(chave);

                    // Verifica se retornou sucesso
                    if (!string.IsNullOrEmpty(respostaXml))
                    {
                        try
                        {
                            var diretorioDaAplicacao = AppDomain.CurrentDomain.BaseDirectory;
                            string caminho = Path.Combine(diretorioDaAplicacao, "NFSe.xml");

                            if (File.Exists(caminho))
                                File.Delete(caminho);

                            File.WriteAllText(caminho, respostaXml);
                            LogService.Log("XML salvo em: " + caminho, Color.Green);
                        }
                        catch (Exception ex)
                        {
                            LogService.Log("Erro ao salvar XML: " + ex.Message, Color.Red);
                        }
                    }
                    else
                    {
                        LogService.Log("Resposta não contem um XML.", Color.Orange);
                    }
                    #endregion

                    #region Consulta da NFSe (PDF)
                    var respostaPdf = servico.ConsultarDanfse(chave);

                    // Verifica se retornou sucesso
                    if (respostaPdf.Length > 4 &&
                        respostaPdf[0] == 0x25 && // %
                        respostaPdf[1] == 0x50 && // P
                        respostaPdf[2] == 0x44 && // D
                        respostaPdf[3] == 0x46)   // F
                    {
                        try
                        {
                            var diretorioDaAplicacao = AppDomain.CurrentDomain.BaseDirectory;
                            string caminho = Path.Combine(diretorioDaAplicacao, "NFSe.pdf");

                            if (File.Exists(caminho))
                                File.Delete(caminho);

                            File.WriteAllBytes(caminho, respostaPdf);
                            LogService.Log("PDF salvo em: " + caminho, Color.Green);
                        }
                        catch (Exception ex)
                        {
                            LogService.Log("Erro ao salvar PDF: " + ex.Message, Color.Red);
                        }
                    }
                    else
                    {
                        LogService.Log("Resposta não contem um PDF.", Color.Orange);
                    }
                    #endregion
                }
                else if (tipoOperacao == "EG")
                {
                    #region DPD = Declaração de Prestação de Serviço
                    if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chave)))
                    {
                        LogService.Log("Arquivo json não encontrado.", Color.Red);
                        return;
                    }

                    var dadosString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chave));
                    var jsonDadosNFSe = JsonDocument.Parse(dadosString);

                    var rpsLote = jsonDadosNFSe.RootElement.GetProperty("RpsLote").GetString();
                    var rpsNumero = jsonDadosNFSe.RootElement.GetProperty("RpsNumero").GetString();
                    var rpsSerie = jsonDadosNFSe.RootElement.GetProperty("RpsSerie").GetString();

                    var prestadorCpfCnpj = jsonDadosNFSe.RootElement.GetProperty("Prestador").GetProperty("CpfCnpj").GetString();
                    var prestadorInscricaoMunicipal = jsonDadosNFSe.RootElement.GetProperty("Prestador").GetProperty("InscricaoMunicipal").GetString();
                    var prestadorMunicipio = jsonDadosNFSe.RootElement.GetProperty("Prestador").GetProperty("Municipio").GetString();

                    var tomadorCpfCnpj = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("CpfCnpj").GetString();
                    var tomadorRazaoSocial = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("RazaoSocial").GetString();
                    var tomadorMunicipio = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Municipio").GetString();
                    var tomadorUF = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("UF").GetString();
                    var tomadorCep = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Cep").GetString();
                    var tomadorLogradouro = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Logradouro").GetString();
                    var tomadorNumero = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Numero").GetString();
                    var tomadorBairro = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Bairro").GetString();
                    var tomadorEmail = jsonDadosNFSe.RootElement.GetProperty("Tomador").GetProperty("Email").GetString();

                    var codigoTributacao = jsonDadosNFSe.RootElement.GetProperty("Servico").GetProperty("CodigoTributacao").GetString();
                    var descricaoServico = jsonDadosNFSe.RootElement.GetProperty("Servico").GetProperty("Descricao").GetString();
                    var informacaoComplementar = jsonDadosNFSe.RootElement.GetProperty("Servico").GetProperty("InformacaoComplementar").GetString();
                    var valorServico = jsonDadosNFSe.RootElement.GetProperty("Servico").GetProperty("Valor").GetDecimal().ToString("F2").Replace(",", ".");
                    var valorIss = jsonDadosNFSe.RootElement.GetProperty("Servico").GetProperty("ValorIss").GetDecimal().ToString("F2").Replace(",", ".");
                    var aliquota = jsonDadosNFSe.RootElement.GetProperty("Servico").GetProperty("Aliquota").GetDecimal().ToString("F2").Replace(",", ".");

                    var dataEmissao = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
                    var dataCompetencia = DateTime.Now.ToString("yyyy-MM-dd");


                    string xmlParaEnviar = string.Empty;

                    #region 1. Criar XML Bruto
                    var xmlRPS = new StringBuilder();
                    xmlRPS.Append("<EnviarLoteRpsEnvio xmlns=\"http://www.ginfes.com.br/servico_enviar_lote_rps_envio_v03.xsd\" xmlns:tipos=\"http://www.ginfes.com.br/tipos_v03.xsd\">");
                    xmlRPS.Append("    <LoteRps Id=\"Lote_" + rpsLote + "\">");
                    xmlRPS.Append("        <tipos:NumeroLote>" + rpsLote + "</tipos:NumeroLote>");
                    xmlRPS.Append("        <tipos:Cnpj>" + prestadorCpfCnpj + "</tipos:Cnpj>");
                    xmlRPS.Append("        <tipos:InscricaoMunicipal>" + prestadorInscricaoMunicipal + "</tipos:InscricaoMunicipal>");
                    xmlRPS.Append("        <tipos:QuantidadeRps>1</tipos:QuantidadeRps>");
                    xmlRPS.Append("        <tipos:ListaRps>");
                    xmlRPS.Append("            <tipos:Rps>");
                    xmlRPS.Append("                <tipos:InfRps Id=\"Rps_" + rpsNumero + "\">");
                    xmlRPS.Append("                    <tipos:IdentificacaoRps>");
                    xmlRPS.Append("                        <tipos:Numero>" + rpsNumero + "</tipos:Numero>");
                    xmlRPS.Append("                        <tipos:Serie>" + rpsSerie + "</tipos:Serie>");
                    xmlRPS.Append("                        <tipos:Tipo>1</tipos:Tipo>");
                    xmlRPS.Append("                    </tipos:IdentificacaoRps>");
                    xmlRPS.Append("                    <tipos:DataEmissao>" + dataEmissao + "</tipos:DataEmissao>");
                    xmlRPS.Append("                    <tipos:NaturezaOperacao>1</tipos:NaturezaOperacao>");
                    xmlRPS.Append("                    <tipos:OptanteSimplesNacional>2</tipos:OptanteSimplesNacional>");
                    xmlRPS.Append("                    <tipos:IncentivadorCultural>2</tipos:IncentivadorCultural>");
                    xmlRPS.Append("                    <tipos:Status>1</tipos:Status>");
                    xmlRPS.Append("                    <tipos:Servico>");
                    xmlRPS.Append("                        <tipos:Valores>");
                    xmlRPS.Append("                            <tipos:ValorServicos>" + valorServico + "</tipos:ValorServicos>");
                    xmlRPS.Append("                            <tipos:ValorPis>0.00</tipos:ValorPis>");
                    xmlRPS.Append("                            <tipos:ValorCofins>0.00</tipos:ValorCofins>");
                    xmlRPS.Append("                            <tipos:ValorInss>0.00</tipos:ValorInss>");
                    xmlRPS.Append("                            <tipos:ValorIr>0.00</tipos:ValorIr>");
                    xmlRPS.Append("                            <tipos:ValorCsll>0.00</tipos:ValorCsll>");
                    xmlRPS.Append("                            <tipos:IssRetido>1</tipos:IssRetido>"); //ISS Retido
                    xmlRPS.Append("                            <tipos:ValorIss>" + valorIss + "</tipos:ValorIss>");
                    xmlRPS.Append("                            <tipos:BaseCalculo>" + valorServico + "</tipos:BaseCalculo>");
                    xmlRPS.Append("                            <tipos:Aliquota>" + aliquota + "</tipos:Aliquota>");
                    xmlRPS.Append("                            <tipos:ValorLiquidoNfse>" + valorServico + "</tipos:ValorLiquidoNfse>");
                    //xmlRPS.Append("                            <tipos:IBSCBS>");
                    //xmlRPS.Append("                              <tipos:finNFSe>0</tipos:finNFSe>");
                    //xmlRPS.Append("                              <tipos:indFinal>1</tipos:indFinal>");
                    //xmlRPS.Append("                              <tipos:cIndOp>100301</tipos:cIndOp>");
                    //xmlRPS.Append("                              <tipos:indDest>0</tipos:indDest>");
                    //xmlRPS.Append("                              <tipos:valores>");
                    //xmlRPS.Append("                                <tipos:trib>");
                    //xmlRPS.Append("                                  <tipos:gIBSCBS>");
                    //xmlRPS.Append("                                    <tipos:CST>000</tipos:CST>");
                    //xmlRPS.Append("                                    <tipos:cClassTrib>000001</tipos:cClassTrib>");
                    //xmlRPS.Append("                                    <tipos:gDif>");
                    //xmlRPS.Append("                                      <tipos:pDifUF>0.10</tipos:pDifUF>");
                    //xmlRPS.Append("                                      <tipos:pDifMun>0.00</tipos:pDifMun>");
                    //xmlRPS.Append("                                      <tipos:pDifCBS>0.90</tipos:pDifCBS>");
                    //xmlRPS.Append("                                    </tipos:gDif>");
                    //xmlRPS.Append("                            		</tipos:gIBSCBS>");
                    //xmlRPS.Append("                            		</tipos:trib>");
                    //xmlRPS.Append("                                 <tipos:cLocalidadeIncid>0000000</tipos:cLocalidadeIncid>");
                    //xmlRPS.Append("                                 <tipos:pRedutor>0.00</tipos:pRedutor>");
                    //xmlRPS.Append("                                 <tipos:TotTribFed>0.00</tipos:TotTribFed>");
                    //xmlRPS.Append("                              </tipos:valores>");
                    //xmlRPS.Append("                            </tipos:IBSCBS>");
                    xmlRPS.Append("                        </tipos:Valores>");
                    xmlRPS.Append("                        <tipos:ItemListaServico>1501</tipos:ItemListaServico>");
                    xmlRPS.Append("                        <tipos:CodigoCnae>6201500</tipos:CodigoCnae>");
                    xmlRPS.Append("                        <tipos:CodigoTributacaoMunicipio>150103</tipos:CodigoTributacaoMunicipio>");
                    xmlRPS.Append("                        <tipos:Discriminacao>" + descricaoServico + "</tipos:Discriminacao>");
                    xmlRPS.Append("                        <tipos:CodigoMunicipio>" + prestadorMunicipio + "</tipos:CodigoMunicipio>");
                    xmlRPS.Append("                    </tipos:Servico>");
                    xmlRPS.Append("                    <tipos:Prestador>");
                    xmlRPS.Append("                        <tipos:Cnpj>" + prestadorCpfCnpj + "</tipos:Cnpj>");
                    xmlRPS.Append("                        <tipos:InscricaoMunicipal>" + prestadorInscricaoMunicipal + "</tipos:InscricaoMunicipal>");
                    xmlRPS.Append("                    </tipos:Prestador>");
                    xmlRPS.Append("                    <tipos:Tomador>");
                    xmlRPS.Append("                        <tipos:IdentificacaoTomador>");
                    xmlRPS.Append("                            <tipos:CpfCnpj>");
                    xmlRPS.Append("                                <tipos:Cnpj>" + tomadorCpfCnpj + "</tipos:Cnpj>");
                    xmlRPS.Append("                            </tipos:CpfCnpj>");
                    xmlRPS.Append("                        </tipos:IdentificacaoTomador>");
                    xmlRPS.Append("                        <tipos:RazaoSocial>" + tomadorRazaoSocial + "</tipos:RazaoSocial>");
                    xmlRPS.Append("                        <tipos:Endereco>");
                    xmlRPS.Append("                            <tipos:Endereco>" + tomadorLogradouro + "</tipos:Endereco>");
                    xmlRPS.Append("                            <tipos:Numero>" + tomadorNumero + "</tipos:Numero>");
                    xmlRPS.Append("                            <tipos:Bairro>" + tomadorBairro + "</tipos:Bairro>");
                    xmlRPS.Append("                            <tipos:CodigoMunicipio>" + tomadorMunicipio + "</tipos:CodigoMunicipio>");
                    xmlRPS.Append("                            <tipos:Uf>" + tomadorUF + "</tipos:Uf>");
                    xmlRPS.Append("                            <tipos:Cep>" + tomadorCep + "</tipos:Cep>");
                    xmlRPS.Append("                        </tipos:Endereco>");
                    xmlRPS.Append("                    </tipos:Tomador>");
                    xmlRPS.Append("                </tipos:InfRps>");
                    xmlRPS.Append("            </tipos:Rps>");
                    xmlRPS.Append("        </tipos:ListaRps>");
                    xmlRPS.Append("    </LoteRps>");
                    xmlRPS.Append("</EnviarLoteRpsEnvio>");

                    xmlParaEnviar = xmlRPS.ToString();
                    //Remover quebra de linha e tabulação para evitar problemas de validação
                    xmlParaEnviar = xmlParaEnviar.Replace("\n", "").Replace("\r", "").Replace("\t", "");
                    //remover espaços em branco entre as tags
                    xmlParaEnviar = System.Text.RegularExpressions.Regex.Replace(xmlParaEnviar, @">\s+<", "><");
                    #endregion

                    #region 2. Assinatura Digital (Se tiver serial)
                    LogService.Log("Assinando o XML digitalmente...", Color.Blue);
                    var assinador = new AssinadorDigital(caminhoCertificado);

                    // Substitui o XML bruto pelo XML Assinado
                    //xmlParaEnviar = assinador.AssinarXml(xmlParaEnviar, "tipos:InfRps");
                    xmlParaEnviar = assinador.AssinarXml(xmlParaEnviar, "LoteRps");
                    LogService.Log("XML Assinado com sucesso!", Color.Green);
                    #endregion

                    #region 3. Validação XSD (Agora validamos o XML já assinado ou o original)
                    var pathXSD = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Ginfes", "schemas_v301");
                    LogService.Log("Validando Schema XSD...");
                    var validador = new NfseValidator(NfseValidator.Provedor.Ginfes, pathXSD);

                    if (validador.ValidarXml(xmlParaEnviar))
                    {
                        // 4. Envio para API
                        LogService.Log("Enviando para API Ginfes...", Color.Blue);
                        var servicoEnvio = new NfseServiceGinfes(caminhoCertificado);

                        var resposta = servicoEnvio.EnviarLote(xmlParaEnviar);
                        if (resposta.sucesso)
                            LogService.Log("Chave de Acesso: " + resposta.chaveAcesso, Color.Green);
                        else
                            LogService.Log("FALHA NO ENVIO:" + resposta.mensagem, Color.Red);

                    }
                    else
                    {
                        LogService.Log("XML INVÁLIDO:", Color.Red);
                        foreach (var erro in validador.Erros)
                            LogService.Log(erro, Color.Red);
                    }
                    #endregion

                    #endregion
                }
            }
            catch (Exception ex)
            {
                LogService.Log("Erro ao consultar: " + ex.Message, Color.Red);
            }

            // Aguarda 10 segundos antes de finalizar
            Thread.Sleep(10000);
        }
    }
}
