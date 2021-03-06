﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace joc_cu_romani_si_barbari
{
    public class _2DCamera
    {
        private float zoomUpperLimit;
        private float zoomLowerLimit;

        private float _zoom;
        private Matrix _transform;
        private Vector2 _pos;
        private float _rotation;
        private int _viewportWidth;
        private int _viewportHeight;
        private int _worldWidth;
        private int _worldHeight;

        public _2DCamera(int viewportWidth, int viewportHeight, int worldWidth, int worldHeight, float initialZoom, float minZoom, float maxZoom)
        {
            _zoom = initialZoom;
            zoomLowerLimit = minZoom;
            zoomUpperLimit = maxZoom;
            _rotation = 0.0f;
            _pos = Vector2.Zero;
            _viewportWidth = viewportWidth;
            _viewportHeight = viewportHeight;
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;
        }

        #region Properties

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = value;
                if (_zoom < zoomLowerLimit) _zoom = zoomLowerLimit;
                if (_zoom > zoomUpperLimit) _zoom = zoomUpperLimit;
            }
        }

        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        public void Move(Vector2 amount)
        {
            _pos += amount;
        }

        public Vector2 Pos
        {
            get { return _pos; }
            set
            {
                float   leftBarrier = (float)_viewportWidth  * .5f / _zoom;
                float bottomBarrier = (float)_viewportHeight * .5f / _zoom;

                float   topBarrier = _worldHeight - (float)_viewportHeight * .5f / _zoom;
                float rightBarrier = _worldWidth  - (float)_viewportWidth  * .5f / _zoom;
                                
                _pos = value;
                if (_pos.X < leftBarrier)   _pos.X = leftBarrier;
                if (_pos.X > rightBarrier)  _pos.X = rightBarrier;
                if (_pos.Y > topBarrier)    _pos.Y = topBarrier;
                if (_pos.Y < bottomBarrier) _pos.Y = bottomBarrier;
            }
        }

        public void setViewport(int w, int h)
        {
            _viewportWidth = w;
            _viewportHeight = h;
        }

        #endregion

        public Matrix GetTransformation()
        {
            _transform =
               Matrix.CreateTranslation(new Vector3(-_pos.X, -_pos.Y, 0)) *
               Matrix.CreateRotationZ(Rotation) *
               Matrix.CreateScale(new Vector3(Zoom, Zoom, 1)) *
               Matrix.CreateTranslation(new Vector3(_viewportWidth * 0.5f, _viewportHeight * 0.5f, 0));

            return _transform;
        }

        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, this.GetTransformation());
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(this.GetTransformation()));
        }
    }
}
