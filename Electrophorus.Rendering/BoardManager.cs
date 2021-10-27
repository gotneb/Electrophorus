﻿using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Electrophorus.Rendering
{
    public class BoardManager
    {
        private readonly List<CircuitComponent> _components;
        private CircuitComponent _component;
        private readonly SKControl _view;
        // Count how many elements are presents in a node
        private Dictionary<SKPoint, int> _positions = new();

        public BoardManager(SKControl view, Board board)
        {
            _view = view;
            _components = board.Components;

            _view.MouseMove += MouseMove;
            _view.MouseDown += (s, e) =>
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
                        _component.CanMove = true;
                    }
                }
            };

            _view.MouseUp += MouseUp;
        }

        private void MouseUp(object sender, MouseEventArgs e)
        {
            if (_component == null) return;

            _component.NodeIn.IsInside(e);
            _component.NodeOut.IsInside(e);
            _component.CanGrowUp = false;
            _component.CanMove = false;

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
                    c.IsRightConnect= false;
                }
            }

            _view.Refresh();
        }

        private void MouseMove(object sender, MouseEventArgs e)
        {
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
    }
}
