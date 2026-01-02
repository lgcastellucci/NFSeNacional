using NFSe_Nacional.Services;
using System.Drawing;

namespace NFSe_Nacional
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string chave = "35387092203966317000175000000000001726010489905886";

                LogService.Log("Consultando NFSe Chave: " + chave + "...", Color.Blue);

                var servico = new NfseService();

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
