﻿using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Diagnostics;
using System.Windows.Forms;
using SharpCircuit.src;
using lib = SharpCircuit.src;
using System.Collections.Generic;

namespace Electrophorus.Rendering
{
    public enum ElementType
    {
        Active,
        Passive
    }

    public abstract class CircuitComponent : ICircuitComponent
    {
        private SKPoint _start;
        private SKPoint _end;

        protected const int _looseness = 4;
        // Effective body of the referent component to calculate left and right margins
        protected readonly float _body;
        // Sides
        protected float _leftWidth;
        protected float _rightWidth;

        public bool IsLeftConnect { get; set; }
        public bool IsRightConnect { get; set; }

        public ElementType TypeElement { get; }
        public lib.CircuitElement Element { get; protected set; }
        public SKPoint Start
        {
            get => _start;
            set
            {
                _start = value;
                NodeIn.Location = Start;
                NodeOut.Location = End;
            }
        }
        public SKPoint End
        {
            get => _end;
            set
            {
                _end = value;
                NodeIn.Location = Start;
                NodeOut.Location = End;
            }
        }
        public SKPaint Paint { get; set; } = new SKPaint() { Color = SKColors.Gray, StrokeWidth = 4 };
        public int MinimumWidth { get; }
        public Node NodeIn { get; set; } = new();
        public Node NodeOut { get; set; } = new();
        public bool CanGrowUp { get; set; } = false;
        public bool CanMove { get; set; } = false;
        //public virtual double Width { get; }
        public int Height { get; protected set; }

        public List<double> CurrentElapised { get; } = new();

        public CircuitComponent(SKPoint start, int body, ElementType type, int initialWidth = Board.CellSize * 2, int height = 6)
        {
            if (initialWidth < Board.CellSize) throw new Exception("Width must be greather than cell border size");

            TypeElement = type;
            Paint.Style = SKPaintStyle.Stroke;
            _body = body;

            MinimumWidth = initialWidth;
            Height = height;
            Start = start;
            End = new SKPoint(Start.X + initialWidth, Start.Y);
        }

        public virtual void Draw(SKCanvas canvas)
        {
            if (!IsLeftConnect) canvas.DrawCircle(NodeIn.Location, NodeIn.Radius, NodeIn.Paint);
            if (!IsRightConnect) canvas.DrawCircle(NodeOut.Location, NodeOut.Radius, NodeOut.Paint);
        }

        protected void DrawText(SKCanvas canvas, double value, string unit, int dy)
        {
            var start = new SKPoint(Start.X + _leftWidth, Start.Y - dy);
            var textPaint = new SKPaint()
            {
                StrokeWidth = 1,
                TextSize = 18,
                Style = SKPaintStyle.Fill,
                Typeface = SKTypeface.FromFamilyName("Century Gothic", new SKFontStyle(SKFontStyleWeight.Thin, SKFontStyleWidth.ExtraExpanded, SKFontStyleSlant.Upright)),
            };
            canvas.DrawText(SIUnits.Normalize(value, unit), start, textPaint);
        }

        public virtual void GrowUp(SKControl view, MouseEventArgs e)
        {
            var x = (int)(e.X / Board.CellSize) * Board.CellSize;
            //var y = ((int)((e.Y + Board.CellSize / 2) / Board.CellSize)) * Board.CellSize;

            if (NodeOut.Inside)
            {
                var dx = x - End.X;
                if (End.X - Start.X <= MinimumWidth && dx <= 0) return;
                End = new SKPoint(End.X + dx, End.Y);

                CalculateSides();
                view.Refresh();
            }
            else if (NodeIn.Inside)
            {
                var dx = Start.X - x;
                if (End.X - Start.X <= MinimumWidth && dx <= 0) return;

                Start = new SKPoint(Start.X - dx, Start.Y);
                CalculateSides();
                view.Refresh();
            }
        }

        public virtual bool IsInside(MouseEventArgs e) => throw new NotImplementedException();

        public virtual void Move(MouseEventArgs e)
        {
            var x = (e.X / Board.CellSize) * Board.CellSize;
            var y = ((e.Y + Board.CellSize / 2) / Board.CellSize) * Board.CellSize;
            var dx = x - Start.X;
            var dy = y - Start.Y;
            if (Start.Y == End.Y)
            {
                Start = new SKPoint(Start.X + dx, Start.Y + dy);
                End = new SKPoint(End.X + dx, Start.Y);
            }
            else
            {
                Start = new SKPoint(Start.X + dx, Start.Y + dy);
                End = new SKPoint(End.X + dx, End.Y + dy);
            }
        }

        protected void CalculateSides()
        {
            var rest = (End.X - Start.X) - _body;
            _leftWidth = (float)rest / 2;
            _rightWidth = (float)rest / 2;
        }

        public virtual void SaveCurrent()
        {
            CurrentElapised.Add(Element.getCurrent());
        }
    }
}
