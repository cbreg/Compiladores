﻿using Rules.Analyzer;
using Rules.Analyzer.Constants;
using Rules.Analyzer.Exceptions;
using System;
using System.Collections;

namespace Rules.Analyzer
{
    public class Sintatico : Constants.Constants
    {
        public Token CurrentToken { get; private set; }
        public Stack Stack { get; set; }
        private Token PreviousToken { get; set; }
        private Lexico Scanner { get; set; }
        private Semantico SemanticAnalyser { get; set; }

        public Sintatico()
        {
            Stack = new Stack();
        }

        private static bool IsTerminal(int x)
        {
            return x < FIRST_NON_TERMINAL;
        }

        private static bool IsNonTerminal(int x)
        {
            return x >= FIRST_NON_TERMINAL && x < FIRST_SEMANTIC_ACTION;
        }

        private static bool IsSemanticAction(int x)
        {
            return x >= FIRST_SEMANTIC_ACTION;
        }

        private bool Step()
        {
            if (CurrentToken == null)
            {
                int pos = 0;
                if (PreviousToken != null)
                    pos = PreviousToken.Position + PreviousToken.Lexeme.Length;

                CurrentToken = new Token(DOLLAR, "fim de arquivo", pos);
            }

            int x = (int)Stack.Pop();
            int a = CurrentToken.Id;

            if (x == EPSILON)
            {
                return false;
            }
            else if (IsTerminal(x))
            {
                if (x == a)
                {
                    if (Stack.Count == 0)
                        return true;
                    else
                    {
                        PreviousToken = CurrentToken;
                        CurrentToken = Scanner.NextToken();
                        return false;
                    }
                }
                else
                {
                    throw new SyntaticException(PARSER_ERROR[x], CurrentToken.Position);
                }
            }
            else if (IsNonTerminal(x))
            {
                if (PushProduction(x, a))
                    return false;
                else
                    throw new SyntaticException(PARSER_ERROR[x], CurrentToken.Position);
            }
            else if (IsSemanticAction(x))
            {
                try
                {
                    SemanticAnalyser.ExecuteAction(x - FIRST_SEMANTIC_ACTION, PreviousToken);
                }
                catch (Exception ex)
                {
                    throw new SemanticException(ex.Message, CurrentToken.Position);
                }
                return false;
            }

            return false;
        }

        private bool PushProduction(int topStack, int tokenInput)
        {
            int p = PARSER_TABLE[topStack - FIRST_NON_TERMINAL][tokenInput - 1];
            if (p >= 0)
            {
                int[] production = PRODUCTIONS[p];

                // Empilha a produção em ordem reversa
                for (int i = production.Length - 1; i >= 0; i--)
                    Stack.Push(production[i]);

                return true;
            }
            else
                return false;
        }

        public void Parse(Lexico scanner, Semantico semanticAnalyser)
        {
            Scanner = scanner;
            SemanticAnalyser = semanticAnalyser;

            Stack.Clear();
            Stack.Push(DOLLAR);
            Stack.Push(START_SYMBOL);

            CurrentToken = scanner.NextToken();

            while (!Step())
            {
                // Para verificar cada token
            }
        }
    }
}