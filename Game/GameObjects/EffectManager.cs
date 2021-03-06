﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassicUO.Game.Data;
using ClassicUO.Input;
using ClassicUO.Renderer;
using ClassicUO.Utility.Logging;
using Microsoft.Xna.Framework;
using IUpdateable = ClassicUO.Interfaces.IUpdateable;

namespace ClassicUO.Game.GameObjects
{
    class EffectManager : IUpdateable
    {
        private readonly List<GameEffect> _effects = new List<GameEffect>();


        public void Add(GraphicEffectType type, Serial source, Serial target, Graphic graphic, Hue hue, Position srcPos,
            Position targPos, byte speed, ushort duration, bool fixedDir, bool doesExplode, bool hasparticles, GraphicEffectBlendMode blendmode)
        {

            if (hasparticles)
            {
                Log.Message(LogTypes.Warning, "Unhandled particles in an effects packet.");
            }

            GameEffect effect = null;

            switch (type)
            {
                case GraphicEffectType.Moving:
                    if (graphic <= 0)
                        return;

                    effect = new MovingEffect(source, target, srcPos.X, srcPos.Y, srcPos.Z, targPos.X, targPos.Y, targPos.Z, graphic, hue)
                    {
                        Blend = blendmode,
                    };

                    if (doesExplode)
                        effect.AddChildEffect(new AnimatedItemEffect(target, targPos.X, targPos.Y, targPos.Z, 0x36Cb, hue, 9));
                    break;
                case GraphicEffectType.Lightning:
                    effect = new LightningEffect(source, srcPos.X, srcPos.Y, srcPos.Z, hue);
                    break;
                case GraphicEffectType.FixedXYZ:
                    if (graphic <= 0)
                        return;
                    effect = new AnimatedItemEffect(srcPos.X, srcPos.Y, srcPos.Z, graphic, hue, duration)
                    {
                        Blend = blendmode,
                    };
                    break;
                case GraphicEffectType.FixedFrom:
                    if (graphic <= 0)
                        return;
                    effect = new AnimatedItemEffect(source, srcPos.X, srcPos.Y, srcPos.Z, graphic, hue, duration)
                    {
                        Blend = blendmode,
                    };
                    break;
                case GraphicEffectType.ScreenFade:
                    Log.Message(LogTypes.Warning, "Unhandled 'Screen Fade' effect.");
                    break;
                default:
                    Log.Message(LogTypes.Warning, "Unhandled effect.");
                    return;
            }

            if (effect != null)
                Add(effect);
        }

        public void Add(GameEffect effect)
        {
            _effects.Add(effect);
        }

        public void Update(double totalMS, double frameMS)
        {
            for (int i = 0; i < _effects.Count; i++)
            {
                GameEffect effect = _effects[i];

                effect.Update(totalMS, frameMS);

                if (effect.IsDisposed)
                {
                    _effects.RemoveAt(i--);

                    if (effect.Children.Count > 0)
                    {
                        for (int j = 0; j < effect.Children.Count; j++)
                            _effects.Add(effect.Children[j]);
                    }
                }
            }
        }
    }
}
