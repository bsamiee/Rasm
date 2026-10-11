using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Rasm.Imaging.Pixels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TorchSharp;

namespace Rasm.Imaging.Filters;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class FrameHeaders {
    // --- [FRAMES]
    extension(PixelFrame frame) {
        public Mat Header() => new(frame.Size.Height, frame.Size.Width, DepthType.Cv32F, 4, frame.Address, step: 0);

        public Fin<Unit> Filter(Func<Mat, Fin<Unit>> apply) => Try.lift(() => {
            using DisposeScope tensors = torch.NewDisposeScope();
            using Mat header = frame.Header();
            return apply(header);
        }).Run();

        public Fin<Unit> Mutate(Action<Image<RgbaVector>> edit) => Try.lift(() => {
            using Image<RgbaVector> image = new(frame.Size.Width, frame.Size.Height);
            Memory2D<Vector4> view = frame.View;
            Lanes(static (pixels, lanes) => { for (int x = 0; x < pixels.Length; x++) pixels[x] = lanes[x] with { W = 1f }; });
            edit(image);
            Lanes(static (pixels, lanes) => { for (int x = 0; x < pixels.Length; x++) lanes[x] = pixels[x] with { W = lanes[x].W }; });
            void Lanes(Action<Span<Vector4>, Span<Vector4>> copy) =>
                image.ProcessPixelRows(rows => { for (int y = 0; y < rows.Height; y++) copy(MemoryMarshal.Cast<RgbaVector, Vector4>(rows.GetRowSpan(y)), view.Span.GetRowSpan(y)); });
        }).Run();
    }

    // --- [CORRELATION]
    extension(Mat source) {
        public Fin<Unit> Correlate(Mat target, Mat kernel, DepthType depth, PaddingModes padding,
            Option<(long X, long Y)> anchor = default, double delta = 0) =>
            Render(source, target, depth, pixels => Correlation(pixels, Read(kernel).squeeze(0).squeeze(0), padding, anchor) + delta);

        public Fin<Unit> Separable(Mat target, torch.Tensor kernelX, torch.Tensor kernelY, DepthType depth, PaddingModes padding,
            Option<(long X, long Y)> anchor = default, double scale = 1, double delta = 0) =>
            Render(source, target, depth, pixels => (Separation(pixels, kernelX, kernelY, padding, anchor) * scale) + delta);

        public Fin<Unit> Separable(Mat target, Mat kernelX, Mat kernelY, DepthType depth, PaddingModes padding,
            Option<(long X, long Y)> anchor = default, double scale = 1, double delta = 0) =>
            Render(source, target, depth, pixels => (Separation(pixels, Read(kernelX), Read(kernelY), padding, anchor) * scale) + delta);

        public Fin<Unit> Gaussian(Mat target, (long Width, long Height) support, double sigmaX, double sigmaY, PaddingModes padding) =>
            Render(source, target, source.Depth, pixels => Separation(pixels, GaussianWeights(support.Width, sigmaX), GaussianWeights(support.Height, sigmaY), padding, default));

        public Fin<Unit> Derivative(Mat target, DepthType depth, int dx, int dy, int kernelSize, double scale, double delta, PaddingModes padding) =>
            Render(source, target, depth, pixels => {
                using Mat kernelX = new();
                using Mat kernelY = new();
                CvInvoke.GetDerivKernels(kernelX, kernelY, dx, dy, kernelSize, ktype: DepthType.Cv64F);
                return (Separation(pixels, Read(kernelX), Read(kernelY), padding, default) * scale) + delta;
            });

        public Fin<Unit> Laplacian(Mat target, DepthType depth, int kernelSize, double scale, double delta, PaddingModes padding) =>
            Render(source, target, depth, pixels => {
                using Mat kernelX = new();
                using Mat kernelY = new();
                CvInvoke.GetDerivKernels(kernelX, kernelY, 2, 0, kernelSize, ktype: DepthType.Cv64F);
                torch.Tensor derivative = Read(kernelX);
                torch.Tensor smoothing = Read(kernelY);
                return ((Separation(pixels, derivative, smoothing, padding, default) + Separation(pixels, smoothing, derivative, padding, default)) * scale) + delta;
            });

        public Fin<Unit> CrossMaximum(Mat target, PaddingModes padding, double border = 0) =>
            Render(source, target, source.Depth, pixels => {
                const int radius = 1;
                const int diameter = (2 * radius) + 1;
                (long, long) stride = (1, 1);
                torch.Tensor across = torch.nn.functional.max_pool2d(torch.nn.functional.pad(pixels, (radius, radius, 0, 0), padding, border), (1, diameter), stride);
                torch.Tensor down = torch.nn.functional.max_pool2d(torch.nn.functional.pad(pixels, (0, 0, radius, radius), padding, border), (diameter, 1), stride);
                return torch.maximum(across, down);
            });
    }

    public static Fin<torch.Tensor> GaussianKernel(long count, double sigma) => Try.lift(() => {
        using DisposeScope tensors = torch.NewDisposeScope();
        return tensors.MoveToOuter(GaussianWeights(count, sigma));
    }).Run();

    private static torch.Tensor GaussianWeights(long count, double sigma) {
        if (count == 1) return torch.ones(count, dtype: torch.ScalarType.Float64, device: torch.CPU);
        torch.Tensor offsets = (torch.ones(count, dtype: torch.ScalarType.Int64, device: torch.CPU).cumsum(0) - ((count / 2) + 1)).to_type(torch.ScalarType.Float64) / sigma;
        torch.Tensor weights = (offsets.square() * -0.5).exp();
        return weights / weights.sum();
    }

    private static torch.Tensor Separation(torch.Tensor pixels, torch.Tensor kernelX, torch.Tensor kernelY, PaddingModes padding, Option<(long X, long Y)> anchor) =>
        Correlation(Correlation(pixels, kernelX.reshape(1, -1), padding, anchor.Map(static point => (point.X, 0L))),
            kernelY.reshape(-1, 1), padding, anchor.Map(static point => (0L, point.Y)));

    private static torch.Tensor Correlation(torch.Tensor pixels, torch.Tensor kernel, PaddingModes padding, Option<(long X, long Y)> anchor) {
        long height = kernel.size(0);
        long width = kernel.size(1);
        long channels = pixels.size(1);
        (long x, long y) = anchor.IfNone((width / 2, height / 2));
        torch.Tensor padded = torch.nn.functional.pad(pixels, (x, width - x - 1, y, height - y - 1), padding);
        torch.Tensor weights = kernel.to_type(torch.ScalarType.Float64).reshape(1, 1, height, width);
        return torch.nn.functional.conv2d(padded.reshape(channels, 1, padded.size(2), padded.size(3)), weights)
            .reshape(1, channels, pixels.size(2), pixels.size(3));
    }

    // --- [NATIVE_STORAGE]
    private static Fin<Unit> Render(Mat source, Mat target, DepthType depth, Func<torch.Tensor, torch.Tensor> apply) => Try.lift(() => {
        using DisposeScope tensors = torch.NewDisposeScope();
        torch.Tensor pixels = apply(Read(source)).squeeze(0).permute(1, 2, 0).contiguous();
        target.Create(source.Rows, source.Cols, depth, source.NumberOfChannels);
        Transfer(target, pixels, static (image, tensor) => tensor.ConvertTo(image, image.Depth));
    }).Run();

    private static torch.Tensor Read(Mat source) {
        torch.Tensor pixels = torch.empty(source.Rows, source.Cols, source.NumberOfChannels, dtype: torch.ScalarType.Float64, device: torch.CPU);
        Transfer(source, pixels, static (image, tensor) => image.ConvertTo(tensor, DepthType.Cv64F));
        return pixels.permute(2, 0, 1).unsqueeze(0).contiguous();
    }

    private static void Transfer(Mat image, torch.Tensor pixels, Action<Mat, Mat> convert) {
        int height = image.Rows;
        int width = image.Cols;
        int channels = image.NumberOfChannels;
        using torch.Storage<double> storage = pixels.storage<double>();
        using Mat tensor = new([height, width, channels], DepthType.Cv64F, storage.data_ptr());
        for (int y = 0; y < height; y++) {
            using Mat imageRow = new([width, channels, 1], image.Depth, image.GetDataPointer(y));
            using Mat tensorRow = new([width, channels, 1], DepthType.Cv64F, tensor.GetDataPointer(y));
            convert(imageRow, tensorRow);
        }
    }
}
