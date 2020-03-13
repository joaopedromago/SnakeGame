using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JogoCobrinha
{
    class Program
    {
        #region Variáveis auxiliáres

        public static char[,] size = new char[15, 15];
        public static int movimentar = 0; //0 = direita, 1 = baixo, 2 = esquerda, 3 = cima
        public static int speed = 200; //miliseconds
        public static int tamanhoInicial = 4;
        public static ConsoleColor corTela = ConsoleColor.White;
        public static ConsoleColor corFundo = ConsoleColor.Black;
        public static char[] movimentos = new char[4] { 'D', 'S', 'A', 'W' };//direita,baixo,esquerda,cima
        public static char[] simboloCabecaSnake = new char[4] { '>', 'v', '<', '^' };//direita,baixo,esquerda,cima
        public static char simboloSnake = '■';
        public static char simboloComida = '+';
        public static char simboloCampo = '░';
        public static bool jogoPerdido = false;
        public static bool jogoCancelado = false;
        public static bool pararJogo = false;
        public static bool comidaPega = false;
        public static List<string> snakePositions = new List<string>();
        public static string posicaoComida = "";
        public static bool morteParede = false;

        #endregion

        #region Game
        static void Main(string[] args)
        {
            IniciarJogo();
        }

        public static void Start()
        {
            SetDefaultSnakePositions();
        }

        #endregion

        #region Redenização

        public static void ExecutarRenders()
        {
            while (!pararJogo)
            {
                LerMovimentos();
                Movimentar();
                if (jogoPerdido && !jogoCancelado)
                {
                    ExibirMensagemJogoPerdido();
                    pararJogo = true;
                }
                else if (!jogoPerdido && !jogoCancelado)
                {
                    Render();
                }
                else
                {
                    var a = 1;
                }
            }
            IniciarJogo();
        }
        public static void LerMovimentos()
        {
            var beginWait = DateTime.Now;
            int digito = -1;
            bool movimentoApertado = false;

            while (DateTime.Now.Subtract(beginWait).TotalMilliseconds < speed)
            {
                if (!movimentoApertado)
                {
                    if (Console.KeyAvailable)
                    {
                        var ch = Console.ReadKey(true).Key;

                        switch (ch)
                        {
                            case ConsoleKey.UpArrow:
                                digito = 3;
                                break;
                            case ConsoleKey.LeftArrow:
                                digito = 2;
                                break;
                            case ConsoleKey.DownArrow:
                                digito = 1;
                                break;
                            case ConsoleKey.RightArrow:
                                digito = 0;
                                break;
                            case ConsoleKey.Escape:
                                jogoCancelado = true;
                                pararJogo = true;
                                return;
                            default:
                                if (char.TryParse(ch.ToString().ToUpper(), out char valor))
                                {
                                    digito = Array.IndexOf(movimentos, valor);
                                }
                                break;
                        }

                        if (ValidarMovimento(digito))
                        {
                            movimentar = digito;
                            movimentoApertado = true;
                        }
                    }
                }
            }
        }

        public static bool ValidarMovimento(int digito)
        {
            return digito != movimentar && digito != -1 && movimentar % 2 != digito % 2;
        }
        public static void Render()
        {
            var tela = "";
            for (var l = 0; l < size.GetLength(0); l++)
            {
                for (var c = 0; c < size.GetLength(1); c++)
                {
                    tela += $"{size[l, c]} ";
                }
                tela += "\n";
            }
            ExibirTela(tela);
        }

        public static void ExibirTela(string tela)
        {
            var titulo = "JOGO DA COBRINHA ~";
            var pontuacao = $"Pontuação Atual: {snakePositions.Count - tamanhoInicial}";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(titulo);

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(pontuacao);

            Console.ForegroundColor = corTela;
            Console.WriteLine(tela);
            Console.ForegroundColor = ConsoleColor.White;

            //Console.Clear();

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, 0);
        }

        public static void aplicarValorPosicao(int l, int c)
        {
            if (ValidarPosicoesSnake(l, c))
            {
                size[l, c] = simboloSnake;
            }
            else if (ValidarPosicoesComida(l, c))
            {
                size[l, c] = simboloComida;
            }
            else
            {
                size[l, c] = simboloCampo;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="l">linha</param>
        /// <param name="c">coluna</param>
        /// <param name="tipoCampo">0 = snake, 1 = comida, 2 = campo</param>
        public static void aplicarValorPosicao(int l, int c, short tipoCampo)
        {
            switch (tipoCampo)
            {
                case 0:
                    size[l, c] = simboloSnake;
                    break;
                case 1:
                    size[l, c] = simboloComida;
                    break;
                case 2:
                    size[l, c] = simboloCampo;
                    break;
                case 3:
                    size[l, c] = simboloCabecaSnake[movimentar];
                    break;
            }
        }

        public static bool ValidarPosicoesSnake(int l, int c)
        {
            if (snakePositions.Contains(l + "/" + c))
            {
                return true;
            }
            return false;
        }
        public static bool ValidarPosicoesComida(int l, int c)
        {
            if (posicaoComida == (l + "/" + c))
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Movimentacao
        //^
        public static void Movimentar()
        {
            switch (movimentar)
            {
                case 0:
                    MovimentarParaDireita();
                    break;
                case 1:
                    MovimentarParaBaixo();
                    break;
                case 2:
                    MovimentarParaEsquerda();
                    break;
                case 3:
                    MovimentarParaCima();
                    break;
            }
        }
        public static void MovimentarParaCima()
        {
            RemoverUltimaPosicao();

            var firstPositions = snakePositions.FirstOrDefault();

            var i = int.Parse(firstPositions.Split("/")[0]) - 1;
            var j = int.Parse(firstPositions.Split("/")[1]);

            ValidarMovimento(i, j);
        }

        //v
        public static void MovimentarParaBaixo()
        {
            RemoverUltimaPosicao();

            var firstPositions = snakePositions.FirstOrDefault();

            var i = int.Parse(firstPositions.Split("/")[0]) + 1;
            var j = int.Parse(firstPositions.Split("/")[1]);

            ValidarMovimento(i, j);
        }

        //>
        public static void MovimentarParaDireita()
        {
            RemoverUltimaPosicao();

            var firstPositions = snakePositions.FirstOrDefault();

            var i = int.Parse(firstPositions.Split("/")[0]);
            var j = int.Parse(firstPositions.Split("/")[1]) + 1;

            ValidarMovimento(i, j);
        }


        //<
        public static void MovimentarParaEsquerda()
        {
            RemoverUltimaPosicao();

            var firstPositions = snakePositions.FirstOrDefault();

            var i = int.Parse(firstPositions.Split("/")[0]);
            var j = int.Parse(firstPositions.Split("/")[1]) - 1;

            ValidarMovimento(i, j);
        }


        public static void RemoverUltimaPosicao()
        {
            if (!comidaPega)
            {
                var lcsnake = snakePositions.LastOrDefault().Split("/");
                aplicarValorPosicao(int.Parse(lcsnake[0]), int.Parse(lcsnake[1]), 2);
                snakePositions.RemoveAt(snakePositions.Count - 1);
            }
            else
            {
                ResetarComida();
            }
        }

        public static void SetNovaPosicao(int i, int j)
        {
            var novaPosicao = i + "/" + j;
            var segundaPosicao = snakePositions.FirstOrDefault();
            var lcSegundaPosicao = segundaPosicao.Split("/");

            aplicarValorPosicao(int.Parse(lcSegundaPosicao[0]), int.Parse(lcSegundaPosicao[1]), 0);

            snakePositions.Insert(0, novaPosicao);

            ValidarComidaPega(novaPosicao);

            aplicarValorPosicao(i, j, 3);
        }

        #endregion

        #region Validação


        public static bool ValidarMorte(int i, int j)
        {
            var validarBaterNelaMesma = !snakePositions.Contains(i + "/" + j);
            return validarBaterNelaMesma;
        }
        public static bool ValidarMorteComParede(int i, int j)
        {
            var validarCimaBaixo = i >= 0 && i < size.GetLength(0);
            var validarEsquerdaDireita = j >= 0 && j < size.GetLength(1);
            var validarBaterNelaMesma = !snakePositions.Contains(i + "/" + j);
            return validarCimaBaixo && validarEsquerdaDireita && validarBaterNelaMesma;
        }

        public static bool ValidarMovimento(int i, int j)
        {
            if (morteParede)
            {
                if (!ValidarMorteComParede(i, j))
                {
                    jogoPerdido = true;
                    return false;
                }

                SetNovaPosicao(i, j);
            }
            else
            {
                if (!ValidarMorte(i, j))
                {
                    jogoPerdido = true;
                    return false;
                }
                if (!ValidarParede(i, j))
                {
                    SetNovaPosicao(i, j);
                }
            }

            return true;
        }

        public static bool ValidarParede(int i, int j)
        {
            var validarCimaBaixo = i >= 0 && i < size.GetLength(0);
            var validarEsquerdaDireita = j >= 0 && j < size.GetLength(1);
            if (!(validarCimaBaixo && validarEsquerdaDireita))
            {
                if (i < 0) // cima
                {
                    i = size.GetLength(0) - 1;
                }
                else if (i >= size.GetLength(0)) // baixo
                {
                    i = 0;
                }
                else if (j < 0) // esquerda
                {
                    j = size.GetLength(1) - 1;
                }
                else //direita
                {
                    j = 0;
                }
                SetNovaPosicao(i, j);

                return true;
            }

            return false;
        }

        public static void ExibirMensagemJogoPerdido()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Red;
            var mensagemErro =
@"
░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░█▌░░█▌░░░█▌█▌░░░░░█▌█▌░█▌█▌█▌░░░░░░░░░░░░
░░░░░░░░░░░░░░░█▌░░█▌░█▌░░░░█▌░█▌░░░░░█▌░░░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░█▌░░█▌░█▌░░░░█▌░█▌░░░░░█▌█▌█▌░░░░░░░░░░░░
░░░░░░░░░░░░░░░█▌░░█▌░█▌░░░░█▌░█▌░░░░░█▌░░░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░░░█▌░░░░░█▌█▌░░░░░█▌█▌░█▌█▌█▌░░░░░░░░░░░░
░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
░░░█▌░░█▌░░░░░█▌█▌░░░░░█▌█▌░░░░░░█▌█▌░░░█▌█▌█▌░█▌░░░░█▌░
░█▌░░█▌░░█▌░█▌░░░░█▌░█▌░░░░█▌░░█▌░░░░█▌░█▌░░░░░█▌░░░░█▌░
░█▌░░█▌░░█▌░█▌░░░░█▌░█▌█▌█▌░░░░█▌█▌█▌░░░█▌█▌█▌░█▌░░░░█▌░
░█▌░░█▌░░█▌░█▌░░░░█▌░█▌░░█▌░░░░█▌░░█▌░░░█▌░░░░░█▌░░░░█▌░
░█▌░░░░░░█▌░░░█▌█▌░░░█▌░░░░█▌░░█▌░░░░█▌░█▌█▌█▌░░░█▌█▌░░░
░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░███████████████████████████░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░████▀░░░░░░░░░░░░░░░░░▀████░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░███│░░░░░░░░░░░░░░░░░░░│███░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░██▌│░░░░░░░░░░░░░░░░░░░│▐██░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░██░└┐░░░░░░░░░░░░░░░░░┌┘░██░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░██░░└┐░░░░░░░░░░░░░░░┌┘░░██░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░██░░┌┘▄▄▄▄▄░░░░░▄▄▄▄▄└┐░░██░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░██▌░│██████▌░░░▐██████│░▐██░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░███░│▐███▀▀░░▄░░▀▀███▌│░███░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░██▀─┘░░░░░░░▐█▌░░░░░░░└─▀██░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░██▄░░░▄▄▄▓░░▀█▀░░▓▄▄▄░░░▄██░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░████▄─┘██▌░░░░░░░▐██└─▄████░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░█████░░▐█─┬┬┬┬┬┬┬─█▌░░█████░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░████▌░░░▀┬┼┼┼┼┼┼┼┬▀░░░▐████░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░█████▄░░░└┴┴┴┴┴┴┴┘░░░▄█████░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░███████▄░░░░░░░░░░░▄███████░░░░░░░░░░░░░░
░░░░░░░░░░░░░░░███████████████████████████░░░░░░░░░░░░░░";

            Console.WriteLine(mensagemErro);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Tamanho final: {snakePositions.Count + 1}");
            Thread.Sleep(2000);
            Console.WriteLine("Aperte qualquer tecla para continuar");
            Console.ReadKey();

            Console.BackgroundColor = corFundo;
        }

        public static void IniciarJogo()
        {
            ExibirMenu();
            ExecutarRenders();
        }
        #endregion

        #region Mudanças Cobra e Comida

        public static void SetDefaultSnakePositions()
        {
            var snakeLength = ObterInteiro(size.GetLength(0) / 2) < tamanhoInicial ? ObterInteiro(size.GetLength(0) / 2) : tamanhoInicial;

            movimentar = 0;
            jogoPerdido = false;
            pararJogo = false;
            jogoCancelado = false;
            snakePositions = new List<string>();

            for (var i = snakeLength; i > 0; i--)
            {
                snakePositions.Add(ObterInteiro(size.GetLength(1) / 2) + "/" + (ObterInteiro(size.GetLength(0) / 2) - tamanhoInicial + i));
            }


            ResetarComida();
        }

        public static void ValidarComidaPega(string snakePosition)
        {
            if (posicaoComida.Equals(snakePosition))
            {
                comidaPega = true;
            }
        }

        public static void GerarComida()
        {
            GerarPosicaoAleatoria();

            while (snakePositions.Contains(posicaoComida))
            {
                GerarPosicaoAleatoria();
            }

            var lcposicaoComida = posicaoComida.Split("/");
            aplicarValorPosicao(int.Parse(lcposicaoComida[0]), int.Parse(lcposicaoComida[1]), 1);
        }

        public static void GerarPosicaoAleatoria()
        {
            Random rnd = new Random();

            var posicaoIComida = rnd.Next(0, size.GetLength(0));
            var posicaoJComida = rnd.Next(0, size.GetLength(1));
            SetPosicaoComida(posicaoIComida, posicaoJComida);
        }

        public static void SetPosicaoComida(int i, int j)
        {
            posicaoComida = i + "/" + j;
        }

        public static void ResetarComida()
        {
            GerarComida();
            comidaPega = false;
        }

        #endregion

        #region Menu

        public static void ExibirMenu()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("JOGO DA COBRINHA ~");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("1 - Jogar");
            Console.WriteLine("2 - Opções");
            Console.WriteLine("3 - Sair");

            Console.WriteLine("Digite sua opção: ");
            var opcao = Console.ReadLine();
            switch (opcao)
            {
                case "1":
                    Console.Clear();
                    Jogar();
                    return;
                case "2":
                    Console.Clear();
                    Opcoes();
                    break;
                case "3":
                    Sair();
                    break;
            }
            ExibirMenu();
        }

        public static void Jogar()
        {
            Start();
            //redenizando tela
            for (var l = 0; l < size.GetLength(0); l++)
            {
                for (var c = 0; c < size.GetLength(1); c++)
                {
                    aplicarValorPosicao(l, c);
                }
            }
        }

        public static void Opcoes()
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("JOGO DA COBRINHA ~");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("1 - Mudar tamanho inicial da cobra");
            Console.WriteLine("2 - Mudar tamanho da tela");
            Console.WriteLine("3 - Definir evento dos cantos");
            Console.WriteLine("4 - Definir velocidade cobra");
            Console.WriteLine("5 - Definir cor da tela");
            Console.WriteLine("6 - Definir cor do fundo");
            Console.WriteLine("7 - Voltar");

            Console.WriteLine("Digite sua opção: ");

            var opcao = Console.ReadLine();
            switch (opcao)
            {
                case "1":
                    MudarTamanhoInicialCobra();
                    break;
                case "2":
                    MudarTamanhoTela();
                    break;
                case "3":
                    DefinirEventoCantos();
                    break;
                case "4":
                    DefinirVelocidadeCobra();
                    break;
                case "5":
                    DefinirCorDaTela();
                    break;
                case "6":
                    DefinirCorDaFundo();
                    break;
                case "7":
                    return;
            }
            Opcoes();
        }

        public static void Sair()
        {
            Environment.Exit(0);
        }

        #endregion

        #region OpcoesCobra
        public static void MudarTamanhoInicialCobra()
        {
            Console.Clear();
            Console.WriteLine("Mudar tamanho inicial da cobra, atual = " + tamanhoInicial);
            Console.WriteLine("Digite o tamanho Inicial desejado (qualquer cosia que não seja número volta ao menu opções): ");
            var tamanho = Console.ReadLine();
            if (int.TryParse(tamanho, out int novoTamanhoInicial))
            {
                tamanhoInicial = novoTamanhoInicial;
            }
        }
        public static void DefinirVelocidadeCobra()
        {
            Console.Clear();
            Console.WriteLine($"Mudar velocidade da cobra, atual = {speed} ms");
            Console.WriteLine("Digite o tamanho Inicial desejado (qualquer cosia que não seja número volta ao menu opções): ");
            var velocidade = Console.ReadLine();
            if (int.TryParse(velocidade, out int novaVelocidade))
            {
                speed = novaVelocidade;
            }
        }
        public static void MudarTamanhoTela()
        {
            int l = size.GetLength(0), c = size.GetLength(1);
            var validarL = false;
            var validarC = false;

            Console.Clear();
            Console.WriteLine($"Mudar tamanho da tela tela atual: {l}x{c}, digite algo que não seja número para cancelar a qualquer momento");
            while (!validarL)
            {
                Console.WriteLine($"Digite o número de linhas - atual = {l}");
                var tamanho = Console.ReadLine();
                if (int.TryParse(tamanho, out l))
                {
                    validarL = true;
                }
                else
                {
                    return;
                }
            }
            while (!validarC)
            {
                Console.WriteLine($"Digite o número de colunas - atual = {c}");
                var tamanho = Console.ReadLine();
                if (int.TryParse(tamanho, out c))
                {
                    validarC = true;
                }
                else
                {
                    return;
                }
            }
            size = new char[l, c];
        }
        public static void DefinirEventoCantos()
        {
            Console.Clear();
            Console.WriteLine("Definir evento dos cantos");
            Console.WriteLine("1 - Morte ao bater nas paredes");
            Console.WriteLine("2 - Ultrapassar paredes");
            Console.WriteLine("3 - Voltar");

            var resposta = Console.ReadLine();
            switch (resposta)
            {
                case "1":
                    morteParede = true;
                    break;
                case "2":
                    morteParede = false;
                    break;
            }
        }
        public static void DefinirCorDaTela()
        {
            var opcao = "";
            Console.Clear();
            Console.WriteLine($"Definir cor da tela (atual:{corTela.ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("1 - Branco");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("2 - Vermelho");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("3 - Verde");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("4 - Azul");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("5 - Ciano");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("6 - Cinza");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("7 - Magenta");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("8 - Amarelo");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("9 - Vermelho Escuro");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("10 - Verde Escuro");
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("11 - Azul Escuro");
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("12 - Ciano Escuro");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("13 - Cinza Escuro");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("14 - Magenta Escuro");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("15 - Amarelo Escuro");
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine("16 - Preto");

            Console.ForegroundColor = ConsoleColor.White;

            opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    corTela = ConsoleColor.White;
                    break;
                case "2":
                    corTela = ConsoleColor.Red;
                    break;
                case "3":
                    corTela = ConsoleColor.Green;
                    break;
                case "4":
                    corTela = ConsoleColor.Blue;
                    break;
                case "5":
                    corTela = ConsoleColor.Cyan;
                    break;
                case "6":
                    corTela = ConsoleColor.Gray;
                    break;
                case "7":
                    corTela = ConsoleColor.Magenta;
                    break;
                case "8":
                    corTela = ConsoleColor.Yellow;
                    break;
                case "9":
                    corTela = ConsoleColor.DarkRed;
                    break;
                case "10":
                    corTela = ConsoleColor.DarkGreen;
                    break;
                case "11":
                    corTela = ConsoleColor.DarkBlue;
                    break;
                case "12":
                    corTela = ConsoleColor.DarkCyan;
                    break;
                case "13":
                    corTela = ConsoleColor.DarkGray;
                    break;
                case "14":
                    corTela = ConsoleColor.DarkMagenta;
                    break;
                case "15":
                    corTela = ConsoleColor.DarkYellow;
                    break;
                case "16":
                    corTela = ConsoleColor.Black;
                    break;
            }
        }
        public static void DefinirCorDaFundo()
        {
            var opcao = "";
            Console.Clear();
            Console.WriteLine($"Definir cor da Fundo (atual:{corFundo.ToString()})");
            Console.BackgroundColor = ConsoleColor.White;
            Console.WriteLine("1 - Branco");
            Console.BackgroundColor = ConsoleColor.Red;
            Console.WriteLine("2 - Vermelho");
            Console.BackgroundColor = ConsoleColor.Green;
            Console.WriteLine("3 - Verde");
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.WriteLine("4 - Azul");
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.WriteLine("5 - Ciano");
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.WriteLine("6 - Cinza");
            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.WriteLine("7 - Magenta");
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.WriteLine("8 - Amarelo");
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("9 - Vermelho Escuro");
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("10 - Verde Escuro");
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("11 - Azul Escuro");
            Console.BackgroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("12 - Ciano Escuro");
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("13 - Cinza Escuro");
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("14 - Magenta Escuro");
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("15 - Amarelo Escuro");
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine("16 - Preto");

            Console.BackgroundColor = ConsoleColor.White;

            opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    corFundo = ConsoleColor.White;
                    break;
                case "2":
                    corFundo = ConsoleColor.Red;
                    break;
                case "3":
                    corFundo = ConsoleColor.Green;
                    break;
                case "4":
                    corFundo = ConsoleColor.Blue;
                    break;
                case "5":
                    corFundo = ConsoleColor.Cyan;
                    break;
                case "6":
                    corFundo = ConsoleColor.Gray;
                    break;
                case "7":
                    corFundo = ConsoleColor.Magenta;
                    break;
                case "8":
                    corFundo = ConsoleColor.Yellow;
                    break;
                case "9":
                    corFundo = ConsoleColor.DarkRed;
                    break;
                case "10":
                    corFundo = ConsoleColor.DarkGreen;
                    break;
                case "11":
                    corFundo = ConsoleColor.DarkBlue;
                    break;
                case "12":
                    corFundo = ConsoleColor.DarkCyan;
                    break;
                case "13":
                    corFundo = ConsoleColor.DarkGray;
                    break;
                case "14":
                    corFundo = ConsoleColor.DarkMagenta;
                    break;
                case "15":
                    corFundo = ConsoleColor.DarkYellow;
                    break;
                case "16":
                    corFundo = ConsoleColor.Black;
                    break;
            }
            Console.BackgroundColor = corFundo;
        }
        #endregion

        #region Útilitário

        public static int ObterInteiro(double value)
        {
            return int.Parse((value).ToString());
        }

        #endregion
    }
}
