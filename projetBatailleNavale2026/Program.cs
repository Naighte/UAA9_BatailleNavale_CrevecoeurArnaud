using static projetBatailleNavale2026.MethodesDuProjet;

namespace projetBatailleNavale2026
{



    // =====================================================================
    //  PROGRAMME PRINCIPAL
    // =====================================================================

    class Program
    {
        // Palette console
        const ConsoleColor COULEUR_TITRE = ConsoleColor.Yellow;
        const ConsoleColor COULEUR_SUCCES = ConsoleColor.Green;



        // ── ENTRY POINT ──────────────────────────────────────────────────

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            MethodesDuProjet.AfficherBanniere();

            bool rejouer = true;
            while (rejouer)
            {
                MethodesDuProjet.LancerPartie();

                MethodesDuProjet.Ecrire("\n Voulez-vous rejouer ? (O/N) : ", COULEUR_TITRE);
                rejouer = Console.ReadLine()?.Trim().ToUpper() == "O";
            }

            MethodesDuProjet.Ecrire("\n Merci d'avoir joué ! Bonne mer ! ⚓\n\n", COULEUR_SUCCES);
        }
    }
}
