// ИДБ-19-07
// Афанасьев Вадим
// Симплекс-метод

using System;
using System.Collections.Generic;

namespace SimplexMethodCS
{
    class Program
    {
        // Счетчик итераций
        static int Iteration;

        enum EGoal
        {
            Min,
            Max
        }

        static EGoal Goal;

        static void GetMatrInput(ref float[,] matr, int rows, int cols)
        {
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    Console.Write("Enter [{0}][{1}] element: ", i, j);
                    matr[i, j] = float.Parse(Console.ReadLine());
                }
            }
        }

        static void PrintMatr(float [,] matr, int rows, int cols)
        {
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < cols; ++j)
                {
                    var elem = string.Format("{0, 6:0.###}", matr[i, j]);
                    Console.Write(elem + "|");
                }
                Console.WriteLine();
            }
        }

        static int SimplexMethodSolver(ref float[,] matr, int rows, int cols, ref List<int> mainVariables)
        {
            // Симплекс-метод

            // 1. Каноническая форма
            // 2. Линейная функция
            // 
            // На вход подается матрица со значениями
            // 
            // 3. Заполнить 1-ю симплекс таблицу (проверить опорный план)
            // 4. Выбрать ключевой столбец (наим. коэф. индексной строки)
            // 5. Оценочные отношения (bi на элемент ключевого столбца)
            // 6. Выбрать ключевую строку (наим. оценочное отношение)
            // 7. Разрешающий элемент – пересечение ключевого столбца и ключевой строки
            // 8. Определить новую основную переменную (в массив со столбцом основных переменных вписать переменную из ключевого столбца)
            // 9. Вторая симп.-табл. (рекурсия), чекнуть опорный план (это индексная строка)
            // 10. Повторить шаги 4-8

            // Проверить индексную строку
            bool bIsOptimal = true;
            switch (Goal)
            {
            // Если надо минимизировать, то ищем положительные элементы
            case EGoal.Min:
                for (int j = 0; j < cols - 1; ++j)
                {
                    if (matr[rows - 1, j] > 0)
                    {
                        bIsOptimal = false;
                        break;
                    }
                }
                break;
            // Если надо максимизировать, то ищем отрицательные элементы
            case EGoal.Max:
                for (int j = 0; j < cols - 1; ++j)
                {
                    if (matr[rows - 1, j] < 0)
                    {
                        bIsOptimal = false;
                        break;
                    }
                }
                break;
            default:
                Console.WriteLine("Something went wrong!");
                return -1;
            }

            // Проверить базис
            // Если базис не является допустимым, то необходимо преобразовать симплекс-таблицу
            bool bIsBasisPseudo = false;
            for (int i = 0; i < rows; ++i)
            {
                if (matr[i, cols - 1] < 0)
                {
                    bIsBasisPseudo = true;
                }
            }

            int keyIndexRow = 0;
            int keyIndexCol = 0;

            // !Pseudo
            float minEstimatedRelation = 0;

            // Pseudo
            float maxAbsBiElem = 0;
            int maxAbsBiElemRow = 0;

            // 3. Если опорный план оптимален, вывести решение
            if (bIsOptimal && !bIsBasisPseudo)
            {
                Console.WriteLine("\nSolution found:");
                for (int i = 0; i < rows; ++i)
                {
                    if (i < mainVariables.Count)
                    {
                        var elem = string.Format("{0, 6:0.###}", matr[i, cols - 1]);
                        Console.Write("x" + (mainVariables[i] + 1) + ": " + elem + "|");
                    }
                    else
                    {
                        var elemF = string.Format("{0, 6:0.###}", matr[i, cols - 1]);
                        Console.Write("F: " + elemF + "|");
                    }
                }

                Console.Write("\n\n\n");
            }
            // 3. Если опорный план не оптимальный, ботать дальше
            else
            {
                if (!bIsBasisPseudo)
                {
                    // 4. Выбрать ключевой столбец (наим. (наиб.) коэф индексной строки)
                    float keyIndexElem = matr[rows - 1, 0];

                    switch (Goal)
                    {
                    case EGoal.Min:
                        for (int j = 0; j < cols; ++j)
                        {
                            if (matr[rows - 1, j] > keyIndexElem)
                            {
                                keyIndexElem = matr[rows - 1, j];
                                keyIndexCol = j;
                            }
                        }
                        break;
                    case EGoal.Max:
                        for (int j = 0; j < cols; ++j)
                        {
                            if (matr[rows - 1, j] < keyIndexElem)
                            {
                                keyIndexElem = matr[rows - 1, j];
                                keyIndexCol = j;
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("Something went wrong!");
                        return -1;
                    }

                    // 5. Оценочные отношения (bi на элемент ключевого столбца), должны быть одинаковые знаки
                    //    Если находим отрицательные значения, то вводим вспомогательный столбец
                    var estimatedRelations = new List<float>();
                    var estimatedRelationsValidIndexes = new List<int>();
                    for (int i = 0; i < rows - 1; ++i)
                    {
                        if (matr[i, keyIndexCol] != 0.0f)
                        {
                            if
                            (
                                (matr[i, keyIndexCol] >= 0 && matr[i, cols - 1] >= 0) ||
                                (matr[i, keyIndexCol] <= 0 && matr[i, cols - 1] <= 0)
                            )
                            {
                                estimatedRelations.Add(matr[i, cols - 1] / matr[i, keyIndexCol]);
                            }
                            else
                            {
                                estimatedRelations.Add(0);
                                estimatedRelationsValidIndexes.Add(i);
                            }
                        }
                        else
                        {
                            estimatedRelations.Add(0);
                            estimatedRelationsValidIndexes.Add(i);
                        }
                    }

                    // 6. Выбрать ключевую строку (наим. оценочное отношение)
                    estimatedRelations.Sort((a, b) => a.CompareTo(b));
                    minEstimatedRelation = estimatedRelations[0];
                    for (int i = 0; i < rows - 1; ++i)
                    {
                        if (estimatedRelations[i] < minEstimatedRelation && !estimatedRelationsValidIndexes.Contains(i))
                        {
                            minEstimatedRelation = estimatedRelations[i];
                            keyIndexRow = i;
                        }
                    }
                }
                else
                {
                    // 5. Выбрать ключевую строку (наиб. по модулю элемент в столбце bi)
                    maxAbsBiElem = matr[0, cols - 1];
                    for (int i = 0; i < rows - 1; ++i)
                    {
                        if (Math.Abs(matr[i, cols - 1]) > maxAbsBiElem)
                        {
                            maxAbsBiElem = Math.Abs(matr[i, cols - 1]);
                            maxAbsBiElemRow = i;
                        }
                    }
                    keyIndexRow = maxAbsBiElemRow;

                    // 5. Оценочные отношения (индексную строку поделить на элементы ключевой строки)
                    var thetaPseudo = new List<float>();
                    for (int j = 0; j < mainVariables.Count; ++j)
                    {
                        thetaPseudo.Add(matr[rows - 1, j] / matr[keyIndexRow, j]);
                    }

                    // 5. Выбрать ключевой столбец (наим. знач. theta)
                    thetaPseudo.Sort((a, b) => b.CompareTo(a));
                    float minThetaPseudoElem = thetaPseudo[0];
                    int minThetaPseudoElemCol = 0;
                    for (int j = 0; j < mainVariables.Count; ++j)
                    {
                        if (thetaPseudo[j] < minThetaPseudoElem)
                        {
                            minThetaPseudoElem = thetaPseudo[j];
                            minThetaPseudoElemCol = j;
                        }
                    }
                    keyIndexCol = minThetaPseudoElemCol;
                }

                // 7. Разрешающий элемент - пересечение ключевого столбца и ключевой строки
                float resolvingItem = matr[keyIndexRow, keyIndexCol];

                // 8. Определить новую основную переменную (в массив со столбцом основных переменных вписать переменную из ключевого столбца)
                mainVariables[keyIndexRow] = keyIndexCol;

                // 9. Вторая симп.-табл. (рекурсия), чекнуть опорный план (это индексная строка)

                //
                // Составляем новую симплекс-таблицу; для этого делим
                // ключевую строку (строку, в которой находится ключевой элемент) на
                // ключевой элемент, а затем из всех остальных строк(включая индексную)
                // вычитаем полученную строку, умноженную на соответствующий элемент
                // ключевого столбца(чтобы все элементы этого столбца, кроме ключевого,
                // стали равны 0).
                //

                // Делим ключевую строку на ключевой элемент
                for (int j = 0; j < cols; ++j)
                {
                    if (resolvingItem == 0.0f)
                    {
                        matr[keyIndexRow, j] = 0.0f;
                    }
                    else
                    {
                        matr[keyIndexRow, j] /= resolvingItem;
                    }
                }

                // Вычитаем полученную строку, умноженную на соответствующий элемент ключевого столбца, из всех остальных строк
                for (int i = 0; i < rows; ++i)
                {
                    float temp = matr[i, keyIndexCol];

                    for (int j = 0; j < cols; ++j)
                    {
                        if (i == keyIndexRow)
                        {
                            continue;
                        }

                        matr[i, j] -= matr[keyIndexRow, j] * temp;
                    }
                }

                ++Iteration;

                if (Iteration >= 500)
                {
                    throw new Exception("Something went wrong!");
                }

                Console.Write("{0} iteration:\n\n", Iteration);
                PrintMatr(matr, rows, cols);

                // 10. Повторить шаги 4-8
                SimplexMethodSolver(ref matr, rows, cols, ref mainVariables);
            }

            return 0;
        }

        static void Main(string[] args)
        {
            //Goal = EGoal.Min;
            //Goal = EGoal.Max;

            Console.Write("Enter goal; 1 – Min, 2 – Max: ");
            int goalTemp = 0;

            while (goalTemp != 1 || goalTemp != 2)
            {
                goalTemp = int.Parse(Console.ReadLine());

                switch (goalTemp)
                {
                case 1:
                    Goal = EGoal.Min;
                    Console.WriteLine("Desired goal: {0}", Goal);
                    goto GoalIsSet;
                case 2:
                    Goal = EGoal.Max;
                    Console.WriteLine("Desired goal: {0}", Goal);
                    goto GoalIsSet;
                default:
                    Console.Write("Something went wrong, try again.\n");
                    Console.Write("Enter goal; 1 – Min, 2 – Max: ");
                    break;
                }
            }

            GoalIsSet:
            {
                Console.Write("Enter row amount: ");
                int rows = int.Parse(Console.ReadLine());

                Console.Write("Enter col amount: ");
                int cols = int.Parse(Console.ReadLine());

                float[,] matr = new float[rows, cols];

                GetMatrInput(ref matr, rows, cols);

                //int rows = 3;
                //int cols = 5;

                //int rows = 4;
                //int cols = 6;

                //int rows = 3;
                //int cols = 4;

                var MainVariables = new List<int>();
                for (int i = 0; i < rows - 1; ++i)
                {
                    MainVariables.Add(i + (rows - 1));
                }

                //var matr = new float[,]
                //{
                //    {  2,  3,  1,  0,  120 },
                //    { -3, -9,  0,  1, -270 },
                //    { -2, -4,  0,  0,    0 }
                //};

                //var matr = new float[,]
                //{
                //    { -2,  3,  1,  0,  0,  6  },
                //    {  1,  3,  0,  1,  0,  15 },
                //    {  3, -1,  0,  0,  1,  15 },
                //    { -1,  3,  0,  0,  0,  0  }
                //};

                //var matr = new float[,]
                //{
                //    {  1,  1,  1,  0,  0,  3  },
                //    { -2,  3,  0,  1,  0,  8  },
                //    {  1, -1,  0,  0,  1,  2  },
                //    { -4,  1,  0,  0,  0,  0  }
                //};

                //var matr = new float[,]
                //{
                //    {  1, -2,  1, -4  },
                //    {  1,  3, -1,  1  },
                //    {  3,  8, -2,  0  }
                //};

                Console.WriteLine("Initial matrix:");
                PrintMatr(matr, rows, cols);
                Console.WriteLine();

                SimplexMethodSolver(ref matr, rows, cols, ref MainVariables);
            }
        }
    }
}
