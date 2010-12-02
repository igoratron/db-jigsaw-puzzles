 namespace DavidWynne.TreeMap.Silverlight.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// TreeMap will create a TreeMap layout based a series of input values
    /// </summary>
    public static class TreeMap
    {
        private static readonly List<Sector> outputSectors = new List<Sector>();

        private static double brushX;

        private static double brushY;

        private static double workAreaHeight;

        private static double workAreaWidth;

        private static double currentHeight;

        private static bool isDrawingVertically;

        /// <summary>
        /// Plots the specified values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="workArea">The work area.</param>
        /// <returns>A list of Sectors detailing how they should be layed out to create the TreeMap.</returns>
        public static List<Sector> Plot(List<InputValue> values, Size workArea)
        {
            Reset(workArea);
            Squarify(PrepareSectors(values), new List<Sector>(), GetWidth());

            return outputSectors;
        }

        /// <summary>
        /// Resets the specified work area.
        /// </summary>
        /// <param name="workArea">The work area.</param>
        private static void Reset(Size workArea)
        {
            outputSectors.Clear();
            brushX = 0;
            brushY = 0;
            workAreaHeight = workArea.Height;
            workAreaWidth = workArea.Width;
            currentHeight = 0;
        }

        /// <summary>
        /// Calculates each Sectors area.
        /// </summary>
        /// <remarks>
        /// Calculate the total area available given the workArea size.
        /// Calculate the percentage (of the sum of all input values), each individual input value represents.
        /// Use that percentage to assign a percentage of the totalArea available to that sector.
        /// </remarks>
        /// <param name="values">Input values.</param>
        /// <returns>The prepared sections</returns>
        private static List<Sector> PrepareSectors(List<InputValue> values)
        {
            // Sort Descending
            values.Sort((x, y) => y.Value.CompareTo(x.Value));

            PairSectors(values);

            double totalArea = workAreaWidth * workAreaHeight;
            double sumOfValues = 0;
            values.ForEach(value => sumOfValues += value.Value);

            List<Sector> sectors = new List<Sector>();

            foreach (InputValue inputValue in values)
            {
                double percentage = (inputValue.Value / sumOfValues) * 100;
                double area = (totalArea / 100) * percentage;

                sectors.Add(
                        new Sector
                            {
                                    Name = inputValue.Name,
                                    OriginalValue = inputValue.Value,
                                    Area = area,
                                    Percentage = percentage,
                                    RelationTo = inputValue.RelationTo
                            });
            }

            return sectors;
        }

        private static void PairSectors(List<InputValue> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i].RelationTo != null)
                {
                    int parentIndex = values.FindIndex((value) => value.Name.Equals(values[i].RelationTo));
                    values.Insert(parentIndex + 1, values[i]);
                    if (parentIndex > i)
                    {
                        values.RemoveAt(i);
                    }
                    else
                    {
                        values.RemoveAt(i + 1);
                    }

                }
            }
        }

        /// <summary>
        /// Recursively processes the values list provided, laying them out into rows of squares.
        /// </summary>
        /// <param name="values">The input values.</param>
        /// <param name="currentRow">The sectors in the current row.</param>
        /// <param name="width">The width of the current row.</param>
        private static void Squarify(IList<Sector> values, List<Sector> currentRow, double width)
        {
            List<Sector> nextIterationPreview = currentRow.ToList();

            if (values.Count > 1)
            {
                nextIterationPreview.Add(values[0]);
            }

            double currentAspectRatio = CalculateAspectRatio(currentRow, width);
            double nextAspectRatio = CalculateAspectRatio(nextIterationPreview, width);

            if (currentAspectRatio == 0 || (nextAspectRatio < currentAspectRatio && nextAspectRatio >= 1))
            {
                currentRow.Add(values[0]);
                values.RemoveAt(0);
                currentHeight = CalculateHeight(currentRow, width);

                if (values.Count > 0)
                {
                    Squarify(values, currentRow, width);
                }
                else
                {
                    LayoutRow(currentRow);
                }
            }
            else
            {
                // Row has reached it's optimum size
                LayoutRow(currentRow);

                // Start the next row, by passing an empty list of row values and recalculating the current width
                Squarify(values, new List<Sector>(), GetWidth());
            }
        }

        /// <summary>
        /// Layouts the row.
        /// </summary>
        /// <param name="rowSectors">The row sectors.</param>
        private static void LayoutRow(IEnumerable<Sector> rowSectors)
        {
            Point brushStartingPoint = new Point(brushX, brushY);

            if (!isDrawingVertically)
            {
                if (workAreaHeight != currentHeight)
                {
                    brushY = workAreaHeight - currentHeight;
                }
            }

            // Draw each sector in the current row
            foreach (Sector sector in rowSectors)
            {
                // Calculate Width & Height
                double width;
                double height;

                if (isDrawingVertically)
                {
                    width = currentHeight;
                    height = sector.Area / currentHeight;

                    if (sector.RelationTo != null)
                    {
                        sector.RelationTo = "West";
                    }
                    else
                    {
                        sector.RelationTo = "None";
                    }

                }
                else
                {
                    width = sector.Area / currentHeight;
                    height = currentHeight;
                    if (sector.RelationTo != null)
                    {
                        sector.RelationTo = "South";
                    }
                    else
                    {
                        sector.RelationTo = "None";
                    }
                }

                sector.Rect = new Rect(brushX, brushY, width, height);
                outputSectors.Add(sector);

                // Reposition brush for the next sector
                if (isDrawingVertically)
                {
                    brushY += height;
                }
                else
                {
                    brushX += width;
                }
            }

            // Finished drawing all sectors in the row
            // Reposition the brush ready for the next row
            if (isDrawingVertically)
            {
                // x increase by width (in vertical, currentHeight is width)
                // y reset to starting position
                brushX += currentHeight;
                brushY = brushStartingPoint.Y;
                workAreaWidth -= currentHeight;
            }
            else
            {
                brushX = brushStartingPoint.X;
                brushY = brushStartingPoint.Y;
                workAreaHeight -= currentHeight;
            }

            currentHeight = 0;
        }

        /// <summary>
        /// Calculates the aspect ratio.
        /// </summary>
        /// <param name="currentRow">The current row.</param>
        /// <param name="width">The width.</param>
        /// <returns>The Aspect Ratio</returns>
        private static double CalculateAspectRatio(List<Sector> currentRow, double width)
        {
            double sumOfAreas = 0;
            currentRow.ForEach(sector => sumOfAreas += sector.Area);

            double maxArea = int.MinValue;
            double minArea = int.MaxValue;

            foreach (Sector sector in currentRow)
            {
                if (sector.Area > maxArea)
                {
                    maxArea = sector.Area;
                }

                if (sector.Area < minArea)
                {
                    minArea = sector.Area;
                }
            }

            double widthSquared = Math.Pow(width, 2);
            double sumOfAreasSqaured = Math.Pow(sumOfAreas, 2);

            double ratio1 = (widthSquared * maxArea) / sumOfAreasSqaured;
            double ratio2 = sumOfAreasSqaured / (widthSquared * minArea);

            return Math.Max(ratio1, ratio2);
        }

        /// <summary>
        /// Calculates the height.
        /// </summary>
        /// <param name="currentRow">The current row.</param>
        /// <param name="width">The width of the current row.</param>
        /// <returns>The height of the current row.</returns>
        private static double CalculateHeight(List<Sector> currentRow, double width)
        {
            double sum = 0;
            currentRow.ForEach(sector => sum += sector.Area);
            return sum / width;
        }

        /// <summary>
        /// Establishes whether to work vertically or horizontally and returns the relevant width
        /// </summary>
        /// <remarks>
        /// When working vertically, "width" is the actual height of the work space.
        /// </remarks>
        /// <returns>Width of the current Row</returns>
        private static double GetWidth()
        {
            if (workAreaHeight > workAreaWidth)
            {
                isDrawingVertically = false;
                return workAreaWidth;
            }

            isDrawingVertically = true;
            return workAreaHeight;
        }
    }
}