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
                if ((args.Length != 1) && (args.Length != 2))
                {
                    Console.WriteLine("Uso: NFSeNacional <caminho_do_certificado.pem> <dados_envio.json>");
                    Console.WriteLine("Uso: NFSeNacional <caminho_do_certificado.pem> <chave_da_nf>");
                    Thread.Sleep(5000);

                    return;
                }


                string caminhoCertificado = args[0];
                string chave = args[1];

                LogService.Log("Caminho certificado: " + caminhoCertificado, Color.Blue);
                LogService.Log("NFSe Chave: " + chave, Color.Blue);


                if ((args.Length == 2) && (chave.EndsWith("json")))
                {
                    //DPD = Declaração de Prestação de Serviço
                    if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chave)))
                    {
                        LogService.Log("Arquivo json não encontrado.", Color.Red);
                        return;
                    }

                    var dadosString = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, chave));
                    var jsonDadosNFSe = JsonDocument.Parse(dadosString);

                    var rpsNumero = jsonDadosNFSe.RootElement.GetProperty("RpsNumero").GetString();
                    var rpsSerie = jsonDadosNFSe.RootElement.GetProperty("RpsSerie").GetString();

                    var prestadorCpfCnpj = jsonDadosNFSe.RootElement.GetProperty("PrestadorCpfCnpj").GetString();
                    var prestadorInscricaoMunicipal = jsonDadosNFSe.RootElement.GetProperty("PrestadorInscricaoMunicipal").GetString();
                    var prestadorMunicipio =jsonDadosNFSe.RootElement.GetProperty("PrestadorMunicipio").GetString();

                    var tomadorCpfCnpj =jsonDadosNFSe.RootElement.GetProperty("TomadorCpfCnpj").GetString();
                    var tomadorRazaoSocial = jsonDadosNFSe.RootElement.GetProperty("TomadorRazaoSocial").GetString();
                    var tomadorMunicipio = jsonDadosNFSe.RootElement.GetProperty("TomadorMunicipio").GetString();
                    var tomadorCep = jsonDadosNFSe.RootElement.GetProperty("TomadorCep").GetString();
                    var tomadorLogradouro = jsonDadosNFSe.RootElement.GetProperty("TomadorLogradouro").GetString();
                    var tomadorNumero =jsonDadosNFSe.RootElement.GetProperty("TomadorNumero").GetString();
                    var tomadorBairro =jsonDadosNFSe.RootElement.GetProperty("TomadorBairro").GetString();
                    var tomadorEmail = jsonDadosNFSe.RootElement.GetProperty("TomadorEmail").GetString();

                    var codigoTributacao =jsonDadosNFSe.RootElement.GetProperty("CodigoTributacao").GetString();
                    var descricaoServico = jsonDadosNFSe.RootElement.GetProperty("DescricaoServico").GetString();
                    var informacaoComplementar =jsonDadosNFSe.RootElement.GetProperty("InformacaoComplementar").GetString();
                    var valorServico = jsonDadosNFSe.RootElement.GetProperty("ValorServico").GetDecimal().ToString("F2").Replace(",", ".");

                    var dataEmissao = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
                    var dataCompetencia = DateTime.Now.ToString("yyyy-MM-dd");


                    //id = DPS + CNPJ do emitente + Código do Município + Série + Número da DPS
                    var idDps = "DPS" + prestadorMunicipio.PadLeft(7, '0') + "2" + prestadorCpfCnpj.PadLeft(14, '0') + rpsSerie.PadLeft(5, '0') + rpsNumero.PadLeft(15, '0');

                    // 1. Ler XML Bruto
                    var xmlDPS = new StringBuilder();
                    xmlDPS.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    xmlDPS.Append("<DPS xmlns=\"http://www.sped.fazenda.gov.br/nfse\" versao=\"1.00\">");
                    xmlDPS.Append("	<infDPS Id=\"" + idDps + "\">");
                    xmlDPS.Append("		<tpAmb>1</tpAmb>"); // 1 - Produção, 2 - Homologação
                    xmlDPS.Append("		<dhEmi>" + dataEmissao + "</dhEmi>");
                    xmlDPS.Append("		<verAplic>ACBrNFSeX-1.00</verAplic>"); // Versão do sistema emissor
                    xmlDPS.Append("		<serie>" + rpsSerie + "</serie>");
                    xmlDPS.Append("		<nDPS>" + rpsNumero + "</nDPS>");
                    xmlDPS.Append("		<dCompet>" + dataCompetencia + "</dCompet>");
                    xmlDPS.Append("		<tpEmit>1</tpEmit>"); // 1 - Emissão pelo próprio prestador
                    xmlDPS.Append("		<cLocEmi>" + prestadorMunicipio + "</cLocEmi>");
                    xmlDPS.Append("		<prest>");
                    xmlDPS.Append("			<CNPJ>" + prestadorCpfCnpj + "</CNPJ>");
                    xmlDPS.Append("			<IM>" + prestadorInscricaoMunicipal + "</IM>");
                    xmlDPS.Append("			<regTrib>");
                    xmlDPS.Append("				<opSimpNac>1</opSimpNac>"); // 1 - Optante Simples Nacional
                    xmlDPS.Append("				<regEspTrib>0</regEspTrib>"); // 0 - Não se enquadra em nenhuma das opções
                    xmlDPS.Append("			</regTrib>");
                    xmlDPS.Append("		</prest>");
                    xmlDPS.Append("		<toma>");
                    xmlDPS.Append("			<CNPJ>" + tomadorCpfCnpj + "</CNPJ>");
                    xmlDPS.Append("			<xNome>" + tomadorRazaoSocial + "</xNome>");
                    xmlDPS.Append("			<end>");
                    xmlDPS.Append("				<endNac>");
                    xmlDPS.Append("					<cMun>" + tomadorMunicipio + "</cMun>");
                    xmlDPS.Append("					<CEP>" + tomadorCep + "</CEP>");
                    xmlDPS.Append("				</endNac>");
                    xmlDPS.Append("				<xLgr>" + tomadorLogradouro + "</xLgr>");
                    xmlDPS.Append("				<nro>" + tomadorNumero + "</nro>");
                    xmlDPS.Append("				<xBairro>" + tomadorBairro + "</xBairro>");
                    xmlDPS.Append("			</end>");
                    xmlDPS.Append("			<email>" + tomadorEmail + "</email>");
                    xmlDPS.Append("		</toma>");
                    xmlDPS.Append("		<serv>");
                    xmlDPS.Append("			<locPrest>");
                    xmlDPS.Append("				<cLocPrestacao>" + prestadorMunicipio + "</cLocPrestacao>");
                    xmlDPS.Append("			</locPrest>");
                    xmlDPS.Append("			<cServ>");
                    xmlDPS.Append("				<cTribNac>" + codigoTributacao + "</cTribNac>");
                    xmlDPS.Append("				<xDescServ>" + descricaoServico + "</xDescServ>");
                    xmlDPS.Append("			</cServ>");
                    xmlDPS.Append("			<infoCompl>");
                    xmlDPS.Append("				<xInfComp>" + informacaoComplementar + "</xInfComp>");
                    xmlDPS.Append("			</infoCompl>");
                    xmlDPS.Append("		</serv>");
                    xmlDPS.Append("		<valores>");
                    xmlDPS.Append("			<vServPrest>");
                    xmlDPS.Append("				<vServ>" + valorServico + "</vServ>");
                    xmlDPS.Append("			</vServPrest>");
                    xmlDPS.Append("			<trib>");
                    xmlDPS.Append("				<tribMun>");
                    xmlDPS.Append("					<tribISSQN>1</tribISSQN>"); // 1 - ISSQN devido no município
                    xmlDPS.Append("					<tpRetISSQN>1</tpRetISSQN>"); // 1 - Não Retido
                    xmlDPS.Append("				</tribMun>");
                    xmlDPS.Append("				<totTrib>");
                    xmlDPS.Append("					<indTotTrib>0</indTotTrib>"); // 0 - Não compõe o total da nota fiscal
                    xmlDPS.Append("				</totTrib>");
                    xmlDPS.Append("			</trib>");
                    xmlDPS.Append("		</valores>");
                    xmlDPS.Append("	</infDPS>");
                    xmlDPS.Append("</DPS>");

                    string xmlParaEnviar = xmlDPS.ToString();

                    // 2. Assinatura Digital (Se tiver serial)
                    LogService.Log("Assinando o XML digitalmente...", Color.Blue);
                    var assinador = new AssinadorDigital(caminhoCertificado);

                    // Substitui o XML bruto pelo XML Assinado
                    xmlParaEnviar = assinador.AssinarDps(xmlParaEnviar);

                    LogService.Log("XML Assinado com sucesso!", Color.Green);


                    // 3. Validação XSD (Agora validamos o XML já assinado ou o original)
                    var pathXSD = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XSD");
                    LogService.Log("Validando Schema XSD...");
                    var validador = new NfseValidator(pathXSD);

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
   
                }

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
            catch (Exception ex)
            {
                LogService.Log("Erro ao consultar: " + ex.Message, Color.Red);
            }

            // Aguarda 10 segundos antes de finalizar
            Thread.Sleep(10000);
        }
    }
}
