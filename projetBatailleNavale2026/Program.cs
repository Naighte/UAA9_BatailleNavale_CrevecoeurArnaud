namespace projetBatailleNavale2026
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MethodesDuProjet mesOutils = new MethodesDuProjet();

            string choixUser = "";        //entrée utilisateur
            int nbLignes;               //nombre de lignes d'une matrice
            int nbColonnes;             //nombre de colonnes d'une matrice
            int[,] matrice;            //première matrice

            do
            {
                //menu
                Console.WriteLine("BATAILLE NAVALE!");
                Console.WriteLine("=================");

                //adversaire
                Console.WriteLine("Comment souhaitez-vous jouer ?");
                Console.WriteLine("1. solo (jouez contre l'ordinateur et entrainez vous)");
                Console.WriteLine("2. multi (jouez avec un amis, tirez chacun votre tour e que le meilleur gagne)");

                //mode de jeu
                Console.WriteLine("Quel mode voulez-vous utiliser ?");
                Console.WriteLine("1. mode classique (bateau placé par le joueur et 1 seul tir par tour)");
                Console.WriteLine("2. mode rapide (bateaux placé aléatoirement et tir continu tant qu'un bateau est touché (arrêt lorsq'un tir rate ou qu'un bateau est coulé))");

                //reprise
                Console.WriteLine("Voulez-vous recommencer ? (oui = 'O' non = autre");
                choixUser = Console.ReadLine();
                Console.Clear();

            } while (choixUser == "O" || choixUser == "o");
        }
    }
}
