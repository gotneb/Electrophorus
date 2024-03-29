﻿using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System;
using System.Windows.Forms;

using lib = SharpCircuit.src;
using Electrophorus.Rendering.Windows;
using Electrophorus.Rendering.Elements;

namespace Electrophorus.Rendering
{
    public class BoardManager
    {
        // Count how many elements are presents in a node
        private readonly Dictionary<SKPoint, int> _positions = new();
        private readonly Stack<CircuitComponent> _components;
        private readonly Timer _timer;
        private readonly SKControl _view;
        private CircuitComponent _component;

        public const double TimeStep = 1e-2;

        public lib.Circuit Circuit { get; set; } = new();

        public BoardManager(SKControl view, Board board)
        {
            _view = view;
            _components = board.Components;

            _view.MouseMove += MouseMove;
            _view.MouseDown += MouseDown;
            _view.DoubleClick += DoubleClick;
            _view.MouseUp += MouseUp;

            Circuit.timeStep = TimeStep;

            // Temporizador
            _timer = new Timer()
            {
                Interval = 1000,
            };
            _timer.Start();
            _timer.Tick += _timer_Tick;
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            Circuit.doTick();

            foreach (var c in _components)
            {
                c.SaveCurrent();
            }
        }

        // Show component informations
        private void DoubleClick(object sender, EventArgs e)
        {
            if (_component == null) return;

            // If it's switch then it changes your state
            if (_component.GetType().ToString().ToLower().Contains("switch"))
            {
                ((SwitchSPST)_component).Toggle();
                _view.Refresh();
                return;
            }
            // Open Plot dialog
            _component.ShowPlot(_view, Circuit);
        }

        private void MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_components.Count < 1) return;

                _component = _components.Where(c => c.IsInside(e)).FirstOrDefault();
                if (_component == null) return;

                if (_component.NodeIn.IsInside(e) || _component.NodeOut.IsInside(e))
                {
                    _component.CanGrowUp = true;
                }
                else
                {
                    _component.CanMove = !_component.IsLeftLocked && !_component.IsRightLocked;
                }
            }
        }

        private void MouseUp(object sender, MouseEventArgs e)
        {
            if (_component == null) return;

            _component.NodeIn.IsInside(e);
            _component.NodeOut.IsInside(e);
            _component.CanGrowUp = false;
            _component.CanMove = false;

            ConnectNodes();

            _view.Refresh();
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
            /*
            foreach (var c in _components)
            {
                if (c.IsAbove(e))
                {
                    _view.Refresh();
                }
            }
            */

            if (_component == null) return;

            if (_component.CanGrowUp)
            {
                _view.Cursor = Cursors.Cross;
                _component.GrowUp(_view, e);
            }
            else if (_component.CanMove)
            {
                _view.Cursor = Cursors.SizeAll;
                MoveComponent(_component, e);
            }
            else
            {
                _view.Cursor = Cursors.Default;
            }
        }

        private void MoveComponent(CircuitComponent c, MouseEventArgs e)
        {
            c.Move(e);
            _view.Refresh();
        }

        // Paint connection between components
        private void ConnectNodes()
        {
            // FIXME: It's so bad clean dictionary all time, but I don't have time to do it now
            _positions.Clear();
            foreach (var c in _components)
            {
                if (!_positions.ContainsKey(c.NodeIn.Location))
                {
                    _positions.Add(c.NodeIn.Location, 1);
                }
                else
                {
                    _positions[c.NodeIn.Location]++;
                }

                if (!_positions.ContainsKey(c.NodeOut.Location))
                {
                    _positions.Add(c.NodeOut.Location, 1);
                }
                else
                {
                    _positions[c.NodeOut.Location]++;
                }
            }

            foreach (var c in _components)
            {
                if (_positions[c.NodeIn.Location] > 1)
                {
                    c.IsLeftConnect = true;
                }
                else
                {
                    c.IsLeftConnect = false;
                }

                if (_positions[c.NodeOut.Location] > 1)
                {
                    c.IsRightConnect = true;
                }
                else
                {
                    c.IsRightConnect = false;
                }
            }

            Control.Connect(_positions, _components, Circuit);
        }
    }
}
