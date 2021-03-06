﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shader;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = ImageFramework.DirectX.Device;

namespace ImageViewer.Controller.TextureViews.Texture3D
{
    using Texture3D = SharpDX.Direct3D11.Texture3D;

    public class RayCastingView : Texture3DBaseView
    {

        private readonly RayCastingShader shader;
        private readonly RayMarchingShader marchingShader;
        private readonly EmptySpaceSkippingShader emptySpaceSkippingShader;
        private RayCastingDisplayModel displayEx;
        private SpaceSkippingTexture3D[] helpTextures;

        public RayCastingView(ModelsEx models) : base(models)
        {
            shader = new RayCastingShader(models);
            marchingShader = new RayMarchingShader(models);
            emptySpaceSkippingShader = new EmptySpaceSkippingShader();
            displayEx = (RayCastingDisplayModel)models.Display.ExtendedViewData;
            helpTextures = new SpaceSkippingTexture3D[models.NumPipelines];
        }

        public override void Draw(int id, ITexture texture)
        {
            if (texture == null) return;

            base.Draw(id, texture);

            var dev = Device.Get();
            dev.OutputMerger.BlendState = models.ViewData.AlphaDarkenState;

            if (models.Display.LinearInterpolation)
            {
                shader.Run(models.Display.ClientAspectRatio, GetWorldToImage(), CalcFarplane(), 
                texture.GetSrView(models.Display.ActiveLayerMipmap), helpTextures[id].GetSrView(models.Display.ActiveMipmap));
            }
            else
            {
                marchingShader.Run(models.Display.ClientAspectRatio, GetWorldToImage(), CalcFarplane(),
                displayEx.FlatShading, texture.GetSrView(models.Display.ActiveLayerMipmap), helpTextures[id].GetSrView(models.Display.ActiveMipmap));
            }

            dev.OutputMerger.BlendState = models.ViewData.DefaultBlendState;
        }

        private Matrix GetWorldToImage()
        {
            float aspectX = models.Images.Size.X / (float)models.Images.Size.Y;
            float aspectZ = models.Images.Size.Z / (float)models.Images.Size.Y;

            return
                Matrix.Translation(-cubeOffsetX, -cubeOffsetY, -GetCubeCenter()) * // translate cube center to origin
                GetRotation() * // undo rotation
                Matrix.Scaling(0.5f * aspectX, 0.5f, 0.5f * aspectZ) * // scale to [-0.5, 0.5]
                Matrix.Translation(0.5f, 0.5f, 0.5f); // move to [0, 1]
        }

        public override Size3 GetTexelPosition(Vector2 mouse)
        {
            return new Size3(0, 0, 0);
        }

        public override void Dispose()
        {
            shader?.Dispose();
            marchingShader?.Dispose();
            emptySpaceSkippingShader?.Dispose();
            foreach (var tex in helpTextures)
            {
                tex?.Dispose();
            }
        }

        public override void UpdateImage(int id, ITexture texture)
        {
            base.UpdateImage(id, texture);

            helpTextures[id]?.Dispose();
            if (texture is null) return;

            SpaceSkippingTexture3D tex = new SpaceSkippingTexture3D(texture.Size, texture.NumMipmaps);
            helpTextures[id] = tex;
            emptySpaceSkippingShader.Execute(texture.GetSrView(LayerMipmapSlice.Mip0), helpTextures[id], texture.Size);

        }

        public class SpaceSkippingTexture3D : ImageFramework.DirectX.Texture3D
        {
            public SpaceSkippingTexture3D(Size3 orgSize, int numMipMaps) : base(numMipMaps, orgSize, Format.R8_UInt, true, false)
            {

            }
        }

    }
}
