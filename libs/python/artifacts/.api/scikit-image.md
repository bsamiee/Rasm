# [PY_ARTIFACTS_API_SCIKIT_IMAGE]

`scikit-image` (`skimage`) owns array-level image processing on the artifacts imaging rail: every domain operation is a pure function over `numpy.ndarray` with no private image class, so the rail composes by passing arrays rather than objects. Decode, encode, and rendering stay outside the boundary — skimage holds the array-transform middle, drawing input from the `pillow`/`tifffile` codec owners and feeding the visualization and mesh tiers downstream.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scikit-image`
- package: `scikit-image` (`BSD-3-Clause`, scikit-image contributors)
- import: `skimage` (submodules lazy-load via `lazy_loader`; each domain module materializes on first attribute access)
- owner: `artifacts`
- rail: imaging
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometric transform models — `skimage.transform`
- type-family: geometric model
- Estimation: `Model.from_estimate(src, dst)` fits from correspondences; `.inverse` gives the inverse map; `+` composes two models.

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                          |
| :-----: | :--------------------------- | :------------------------------------ |
|  [01]   | `AffineTransform`            | scale, shear, rotation, translation   |
|  [02]   | `EuclideanTransform`         | rotation and translation only         |
|  [03]   | `SimilarityTransform`        | uniform scale, rotation, translation  |
|  [04]   | `ProjectiveTransform`        | full homography via DLT               |
|  [05]   | `EssentialMatrixTransform`   | essential matrix from correspondences |
|  [06]   | `FundamentalMatrixTransform` | fundamental matrix estimation         |
|  [07]   | `PolynomialTransform`        | polynomial warp model                 |
|  [08]   | `PiecewiseAffineTransform`   | triangulated piecewise affine warp    |
|  [09]   | `ThinPlateSplineTransform`   | thin-plate spline interpolation warp  |

[PUBLIC_TYPE_SCOPE]: RANSAC fitting models — `skimage.measure`
- type-family: RANSAC model
- Members: each exposes `residuals(data)` and the `from_estimate(data)` classmethod; parametric models add `predict_xy(t)`, `LineModelND` adds `predict_x`/`predict_y`.

| [INDEX] | [SYMBOL]       | [CAPABILITY]                                |
| :-----: | :------------- | :------------------------------------------ |
|  [01]   | `CircleModel`  | circle fit; `params=(xc, yc, r)`            |
|  [02]   | `EllipseModel` | ellipse fit; `params=(xc, yc, a, b, theta)` |
|  [03]   | `LineModelND`  | N-D line fit; `params=(origin, direction)`  |

[PUBLIC_TYPE_SCOPE]: feature descriptors — `skimage.feature`

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY]       | [CAPABILITY]                                            |
| :-----: | :-------- | :------------------ | :------------------------------------------------------ |
|  [01]   | `SIFT`    | keypoint descriptor | scale-invariant feature transform; `detect_and_extract` |
|  [02]   | `ORB`     | keypoint descriptor | oriented FAST and rotated BRIEF                         |
|  [03]   | `BRIEF`   | binary descriptor   | binary robust independent elementary features           |
|  [04]   | `CENSURE` | keypoint detector   | center-surround extremas detector                       |
|  [05]   | `Cascade` | object detector     | Haar-like feature cascade classifier                    |

[PUBLIC_TYPE_SCOPE]: lazy I/O collections — `skimage.io`
- type-family: lazy collection

| [INDEX] | [SYMBOL]          | [CAPABILITY]                                  |
| :-----: | :---------------- | :-------------------------------------------- |
|  [01]   | `ImageCollection` | load-on-demand sequence of images from a glob |
|  [02]   | `MultiImage`      | multi-frame image file (GIF/TIFF) collection  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: I/O — `skimage.io`

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `imread(fname, as_gray=False, **plugin_args)`                    | read           | read image file to ndarray              |
|  [02]   | `imsave(fname, arr, *, check_contrast=True, **plugin_args)`      | write          | write ndarray to image file             |
|  [03]   | `ImageCollection(load_pattern, conserve_memory, load_func, ...)` | collection     | lazy-load a glob pattern of image files |

[ENTRYPOINT_SCOPE]: color conversion — `skimage.color`

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY]     | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------- | :----------------- | :---------------------------------------------------- |
|  [01]   | `rgb2gray(rgb)`                                 | colorspace convert | RGB to luminance float                                |
|  [02]   | `rgb2hsv(rgb)`                                  | colorspace convert | RGB to HSV                                            |
|  [03]   | `rgb2lab(rgb, illuminant, observer, …)`         | colorspace convert | RGB to CIE Lab                                        |
|  [04]   | `lab2rgb(lab, illuminant, observer)`            | colorspace convert | CIE Lab to RGB                                        |
|  [05]   | `gray2rgb(image, channel_axis)`                 | colorspace convert | grayscale to 3-channel RGB                            |
|  [06]   | `label2rgb(label, image, colors, alpha, ...)`   | label colorize     | map integer label mask to RGB overlay                 |
|  [07]   | `convert_colorspace(arr, fromspace, tospace)`   | generic convert    | dispatch colorspace conversion by name string         |
|  [08]   | `deltaE_ciede2000(lab1, lab2, kL, kC, kH)`      | color distance     | CIEDE2000 perceptual color difference                 |
|  [09]   | `separate_stains(rgb, …)` / `combine_stains(…)` | stain separation   | color-deconvolution stain unmixing/remixing (H&E/HDX) |
|  [10]   | `rgb2ycbcr(rgb)` / `ycbcr2rgb(ycbcr)`           | colorspace convert | RGB <-> YCbCr (the broadcast/video colorspace)        |

[ENTRYPOINT_SCOPE]: geometric transform — `skimage.transform`

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]  | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------------- | :-------------- | :------------------------------------------------ |
|  [01]   | `resize(image, output_shape, order, mode, …)`                    | pixel resize    | resize to target shape with anti-aliasing control |
|  [02]   | `rescale(image, scale, order, mode, …)`                          | scale resize    | scale by float factor per-axis                    |
|  [03]   | `rotate(image, angle, resize, center, …)`                        | rotation        | rotate image by degrees                           |
|  [04]   | `warp(image, inverse_map, output_shape, order, …)`               | generic warp    | apply arbitrary inverse coordinate map            |
|  [05]   | `AffineTransform(matrix, *, scale, shear, rotation, …)`          | model construct | build affine transform from parameters            |
|  [06]   | `estimate_transform(ttype, src, dst, **kwargs)`                  | model estimate  | fit a named transform type to correspondences     |
|  [07]   | `hough_line(image, theta)`                                       | Hough           | accumulate Hough line votes                       |
|  [08]   | `hough_circle(image, radius, normalize, full_output)`            | Hough           | Hough circle accumulation                         |
|  [09]   | `radon(image, theta, circle, preserve_range)`                    | Radon           | compute Radon transform (sinogram)                |
|  [10]   | `iradon(radon_image, theta, output_size, filter_name, …)`        | Radon inverse   | filtered back-projection reconstruction           |
|  [11]   | `hough_line_peaks(…)` / `hough_circle_peaks(…)`                  | Hough peaks     | extract peaks from the Hough accumulators         |
|  [12]   | `probabilistic_hough_line(image, threshold, line_length, …)`     | Hough           | probabilistic line-segment detection              |
|  [13]   | `pyramid_gaussian(image, max_layer, …)` / `pyramid_laplacian(…)` | pyramid         | Gaussian / Laplacian image pyramids               |
|  [14]   | `swirl(image, center, …)` / `warp_polar(image, center, …)`       | warp            | swirl warp; log-/linear-polar unwrap              |
|  [15]   | `integral_image(image, *, dtype)` / `matrix_transform(…)`        | util            | summed-area table; point-set matrix apply         |

[ENTRYPOINT_SCOPE]: filtering and thresholding — `skimage.filters`

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CAPABILITY]                                        |
| :-----: | :---------------------------------------------------------------- | :------------- | :-------------------------------------------------- |
|  [01]   | `gaussian(image, sigma, *, mode, cval, truncate, …)`              | smooth         | Gaussian blur; sigma per-axis or scalar             |
|  [02]   | `sobel(image, mask, *, axis, mode, cval)`                         | edge           | Sobel edge magnitude                                |
|  [03]   | `median(image, footprint, out, mode, cval, behavior)`             | smooth         | median filter with structuring element              |
|  [04]   | `threshold_otsu(image, nbins, *, hist)`                           | threshold      | Otsu global threshold value                         |
|  [05]   | `threshold_local(image, block_size, method, offset, …)`           | threshold      | adaptive local threshold array                      |
|  [06]   | `threshold_multiotsu(image, classes, nbins, *, hist)`             | threshold      | multi-class Otsu thresholds                         |
|  [07]   | `gabor(image, frequency, *, theta, bandwidth, …)`                 | frequency      | Gabor filter real and imaginary response            |
|  [08]   | `frangi(image, sigmas, scale_range, …)`                           | vessel filter  | Frangi vessel-enhancement filter                    |
|  [09]   | `unsharp_mask(image, radius, amount, …)`                          | sharpen        | unsharp masking                                     |
|  [10]   | `laplace(image, ksize, mask)`                                     | edge           | Laplace edge detector                               |
|  [11]   | `butterworth(image, cutoff_frequency_ratio, high_pass, order, …)` | frequency      | Butterworth frequency-domain filter                 |
|  [12]   | `scharr` / `prewitt` / `farid` / `roberts`                        | edge           | alternative gradient operators (`_h`/`_v` variants) |
|  [13]   | `meijering(image, sigmas, …)` / `sato(…)` / `hessian(…)`          | ridge filter   | neuriteness/tubeness ridge filters beside `frangi`  |
|  [14]   | `difference_of_gaussians(image, low_sigma, high_sigma, …)`        | bandpass       | DoG band-pass via two Gaussians                     |
|  [15]   | `threshold_li` / `threshold_yen` / `threshold_isodata`            | threshold      | global-threshold family beside Otsu                 |
|  [16]   | `threshold_minimum` / `threshold_triangle` / `threshold_mean`     | threshold      | global-threshold family beside Otsu                 |
|  [17]   | `threshold_niblack(…, k)` / `threshold_sauvola(…, k, r)`          | threshold      | local adaptive document binarization                |
|  [18]   | `try_all_threshold(image, figsize, verbose)`                      | threshold      | apply every global threshold, return a figure       |
|  [19]   | `apply_hysteresis_threshold(image, low, high)`                    | threshold      | two-level hysteresis binarization                   |
|  [20]   | `window(window_type, shape, *, warp_kwargs)`                      | frequency      | n-D apodization window for FFT pre-multiplication   |
|  [21]   | `rank.*` (`mean`/`median`/`maximum`/`entropy`/`autolevel`/…)      | local rank     | footprint-local rank filters on uint8/uint16        |
|  [22]   | `wiener(image, psf, balance, …)` / `correlate_sparse(…)`          | deconvolve     | supervised Wiener deconvolution; sparse correlation |

[ENTRYPOINT_SCOPE]: morphology — `skimage.morphology`

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY]  | [CAPABILITY]                                        |
| :-----: | :------------------------------------------------------------- | :-------------- | :-------------------------------------------------- |
|  [01]   | `binary_erosion(image, footprint, out, *, mode)`               | binary morph    | binary erosion                                      |
|  [02]   | `binary_dilation(image, footprint, out, *, mode)`              | binary morph    | binary dilation                                     |
|  [03]   | `binary_opening(image, footprint, out, *, mode)`               | binary morph    | binary opening                                      |
|  [04]   | `binary_closing(image, footprint, out, *, mode)`               | binary morph    | binary closing                                      |
|  [05]   | `erosion(image, footprint, out, shift_x, shift_y, …)`          | gray morph      | grayscale erosion                                   |
|  [06]   | `dilation(image, footprint, out, shift_x, shift_y, …)`         | gray morph      | grayscale dilation                                  |
|  [07]   | `label(label_image, background, return_num, connectivity)`     | labeling        | connected-component labeling                        |
|  [08]   | `remove_small_objects(ar, connectivity, *, min_size, out)`     | filtering       | remove connected components below min_size          |
|  [09]   | `remove_small_holes(ar, area_threshold, connectivity, …)`      | filtering       | fill holes below area threshold                     |
|  [10]   | `skeletonize(image, *, method)`                                | skeleton        | topological skeleton of binary image                |
|  [11]   | `disk` / `diamond` / `ball` / `ellipse` / `octagon` / `star`   | structuring     | footprint factory shapes (2D/3D)                    |
|  [12]   | `footprint_rectangle(shape, *, dtype, decomposition)`          | structuring     | rectangular / n-D box footprint factory             |
|  [13]   | `flood_fill(image, seed_point, new_value, *, footprint, …)`    | flood fill      | fill connected region with new value                |
|  [14]   | `flood(image, seed_point, *, footprint, connectivity, …)`      | flood           | boolean mask of the flooded region                  |
|  [15]   | `white_tophat` / `black_tophat`                                | gray morph      | bright/dark feature (image minus open/close)        |
|  [16]   | `reconstruction(seed, mask, method, footprint, offset)`        | gray morph      | reconstruction by dilation/erosion (h-maxima)       |
|  [17]   | `area_opening` / `area_closing`                                | attribute morph | attribute filters removing components by area       |
|  [18]   | `diameter_opening` / `diameter_closing`                        | attribute morph | attribute filters removing components by diameter   |
|  [19]   | `max_tree(image, connectivity)` / `max_tree_local_maxima(…)`   | component tree  | max-tree representation for attribute filtering     |
|  [20]   | `medial_axis(image, *, mask, …)` / `thin(image, max_num_iter)` | skeleton        | medial-axis transform; iterative thinning           |
|  [21]   | `convex_hull_image(image, …)` / `convex_hull_object(…)`        | hull            | per-image / per-object 2D convex hull mask          |
|  [22]   | `isotropic_erosion` / `isotropic_dilation`                     | binary morph    | distance-transform radius morphology (no footprint) |
|  [23]   | `isotropic_opening` / `isotropic_closing`                      | binary morph    | distance-transform radius morphology (no footprint) |

[ENTRYPOINT_SCOPE]: segmentation — `skimage.segmentation`

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `watershed(image, markers, connectivity, offset, mask, …)`     | region grow    | watershed from markers                          |
|  [02]   | `slic(image, n_segments, compactness, max_num_iter, …)`        | superpixel     | SLIC superpixel segmentation                    |
|  [03]   | `felzenszwalb(image, scale, sigma, min_size, *, channel_axis)` | graph segment  | Felzenszwalb graph-based segmentation           |
|  [04]   | `active_contour(image, snake, alpha, beta, w_line, w_edge, …)` | contour        | active contour / snake model                    |
|  [05]   | `chan_vese(image, mu, lambda1, lambda2, tol, …)`               | contour        | Chan-Vese level-set segmentation                |
|  [06]   | `random_walker(data, labels, beta, mode, tol, …)`              | probabilistic  | random walker segmentation from seed labels     |
|  [07]   | `find_boundaries(label_img, connectivity, mode, background)`   | boundary       | boolean boundary map from label image           |
|  [08]   | `expand_labels(label_image, distance)`                         | label expand   | expand label regions by Euclidean distance      |
|  [09]   | `quickshift(image, ratio, kernel_size, max_dist, …)`           | superpixel     | quickshift mode-seeking superpixel segmentation |
|  [10]   | `mark_boundaries(image, label_img, color, outline_color, …)`   | overlay        | draw label boundaries over an image             |
|  [11]   | `clear_border(labels, buffer_size, bgval, mask, *, out)`       | label clean    | drop labels touching the image border           |
|  [12]   | `morphological_chan_vese(…)`                                   | level-set      | morphological Chan-Vese snake evolution         |
|  [13]   | `morphological_geodesic_active_contour(…)`                     | level-set      | geodesic active-contour snake evolution         |
|  [14]   | `inverse_gaussian_gradient(image, alpha, sigma)`               | level-set      | edge-stopping map for the morphological snakes  |
|  [15]   | `join_segmentations(s1, s2, …)` / `relabel_sequential(…)`      | label algebra  | intersect two segmentations; compact relabeling |

[ENTRYPOINT_SCOPE]: region measurement — `skimage.measure`

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY] | [CAPABILITY]                                           |
| :-----: | :--------------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `label(label_image, background, return_num, connectivity)` | label          | connected-component labeling                           |
|  [02]   | `regionprops(label_image, intensity_image, cache, …)`      | region props   | region property list per label                         |
|  [03]   | `regionprops_table(label_image, intensity_image, …)`       | region table   | region props as dict of arrays                         |
|  [04]   | `find_contours(image, level, fully_connected, …)`          | contour        | marching-squares iso-contours                          |
|  [05]   | `marching_cubes(volume, level, *, spacing, …)`             | surface mesh   | 3D isosurface mesh extraction                          |
|  [06]   | `ransac(data, model_class, min_samples, …)`                | RANSAC         | robust fitting via RANSAC                              |
|  [07]   | `mesh_surface_area(verts, faces)`                          | mesh metric    | surface area of a triangulated mesh                    |
|  [08]   | `shannon_entropy(image, base)`                             | entropy        | Shannon entropy of image histogram                     |
|  [09]   | `blur_effect(image, h_size=11, channel_axis=None, …)`      | no-ref quality | no-reference perceptual blur (0=sharp … 1=blurred)     |
|  [10]   | `profile_line(image, src, dst, linewidth=1, …)`            | scan line      | 1-D intensity profile along `src`->`dst` (section-cut) |

[ENTRYPOINT_SCOPE]: feature detection and description — `skimage.feature`

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY]   | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------- | :--------------- | :-------------------------------------------- |
|  [01]   | `canny(image, sigma, low_threshold, high_threshold, …)`          | edge detect      | Canny multi-stage edge detector               |
|  [02]   | `hog(image, orientations=9, pixels_per_cell=(8,8), …)`           | descriptor       | HOG vector; `visualize=True` adds `hog_image` |
|  [03]   | `SIFT(upsampling=2, n_octaves=8, n_scales=3, sigma_min=1.6, …)`  | descriptor class | SIFT keypoint + descriptor extraction         |
|  [04]   | `match_descriptors(descriptors1, descriptors2, …)`               | matching         | NN matching (Lowe-ratio + cross-check)        |
|  [05]   | `blob_dog(image, min_sigma, max_sigma, sigma_ratio, …)`          | blob detect      | Difference-of-Gaussian blobs                  |
|  [06]   | `blob_log(image, min_sigma, max_sigma, num_sigma, …)`            | blob detect      | Laplacian-of-Gaussian blobs                   |
|  [07]   | `blob_doh(image, min_sigma, max_sigma, num_sigma, …)`            | blob detect      | Determinant-of-Hessian blobs                  |
|  [08]   | `corner_harris(image, method, k, eps, sigma, *, axis)`           | corner detect    | Harris corner response                        |
|  [09]   | `corner_peaks(image, min_distance, threshold_abs, …)`            | corner peaks     | peaks in corner response map                  |
|  [10]   | `peak_local_max(image, min_distance, threshold_abs, …)`          | local max        | local maxima coordinates                      |
|  [11]   | `corner_subpix` / `corner_fast` / `corner_shi_tomasi`            | corner detect    | subpixel / FAST / Shi-Tomasi corner detectors |
|  [12]   | `corner_kitchen_rosenfeld` / `corner_moravec`                    | corner detect    | Kitchen-Rosenfeld / Moravec corner detectors  |
|  [13]   | `local_binary_pattern(image, P, R, method)`                      | texture          | LBP texture descriptor                        |
|  [14]   | `graycomatrix(image, distances, …)` / `graycoprops(P, prop)`     | texture          | GLCM + Haralick scalar props                  |
|  [15]   | `structure_tensor(image, sigma, mode, cval, *, axis, order)`     | tensor           | structure tensor elements                     |
|  [16]   | `hessian_matrix(…)`/`hessian_matrix_eigvals(…)`/`shape_index(…)` | tensor           | Hessian elements/eigenvalues + shape index    |
|  [17]   | `ORB(n_keypoints=500, fast_threshold=0.08, …)`                   | descriptor class | detect-and-describe (feature-pipeline law)    |
|  [18]   | `BRIEF(descriptor_size=256, patch_size=49, …)`                   | descriptor class | describe given keypoints (`.extract`)         |
|  [19]   | `CENSURE(max_scale=7, mode='DoB', …)`                            | keypoint detect  | detect keypoints, no descriptors (`.detect`)  |
|  [20]   | `daisy(image, step, radius, …)` / `multiscale_basic_features(…)` | descriptor       | DAISY dense descriptor; multiscale stack      |
|  [21]   | `haar_like_feature(…)` / `draw_haar_like_feature(…)`             | Haar             | Haar features for the `Cascade` detector      |
|  [22]   | `fisher_vector(descriptors, gmm)` / `learn_gmm(…)`               | aggregation      | Fisher-vector encoding over a learned GMM     |

[ENTRYPOINT_SCOPE]: exposure and intensity — `skimage.exposure`

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------ | :-------------- | :----------------------------------------------- |
|  [01]   | `equalize_hist(image, nbins, mask)`                                 | histogram eq    | global histogram equalization                    |
|  [02]   | `equalize_adapthist(image, kernel_size, clip_limit, …)`             | CLAHE           | contrast-limited adaptive histogram equalization |
|  [03]   | `rescale_intensity(image, in_range, out_range)`                     | intensity scale | linear intensity rescaling to range              |
|  [04]   | `match_histograms(image, reference, *, channel_axis)`               | histogram match | match image histogram to reference               |
|  [05]   | `adjust_gamma(image, gamma, gain)`                                  | gamma           | gamma correction                                 |
|  [06]   | `adjust_log(image, gain, inv)`                                      | log adjust      | logarithmic intensity adjustment                 |
|  [07]   | `histogram(image, nbins, source_range, normalize, ...)`             | histogram       | image histogram array                            |
|  [08]   | `is_low_contrast(image, fraction_threshold, lower_percentile, ...)` | contrast check  | detect low-contrast images                       |
|  [09]   | `adjust_sigmoid(image, cutoff, gain, inv)`                          | sigmoid         | sigmoid (S-curve) contrast adjustment            |
|  [10]   | `cumulative_distribution(image, nbins)`                             | histogram       | image CDF and bin centers                        |

[ENTRYPOINT_SCOPE]: restoration and denoising — `skimage.restoration`

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                   |
| :-----: | :--------------------------------------------------------------------- | :------------- | :--------------------------------------------- |
|  [01]   | `denoise_bilateral(image, win_size, sigma_color, sigma_spatial, …)`    | denoise        | bilateral filter; edge-preserving denoising    |
|  [02]   | `denoise_nl_means(image, patch_size, patch_distance, h, …)`            | denoise        | non-local means denoising                      |
|  [03]   | `denoise_wavelet(image, sigma, wavelet, mode, wavelet_levels, …)`      | denoise        | wavelet-domain denoising                       |
|  [04]   | `denoise_tv_chambolle(image, weight, eps, max_num_iter, channel_axis)` | denoise        | total-variation Chambolle denoising            |
|  [05]   | `estimate_sigma(image, average_sigmas, *, channel_axis)`               | noise estimate | estimate noise standard deviation              |
|  [06]   | `inpaint_biharmonic(image, mask, *, split_into_regions, …)`            | inpaint        | biharmonic inpainting of masked regions        |
|  [07]   | `richardson_lucy(image, psf, num_iter, clip, filter_epsilon, …)`       | deconvolve     | Richardson-Lucy deconvolution                  |
|  [08]   | `rolling_ball(image, *, radius, kernel, nansafe, num_threads)`         | background     | rolling-ball background subtraction            |
|  [09]   | `unsupervised_wiener(image, psf, reg, …)` / `wiener(…)`                | deconvolve     | self-tuned / supervised Wiener-Hunt deconvolve |
|  [10]   | `unwrap_phase(image, wrap_around=False, rng=None)`                     | phase          | 2D/3D phase unwrapping                         |
|  [11]   | `calibrate_denoiser(image, denoise_function, denoise_parameters, …)`   | denoise tune   | J-invariant denoiser parameter calibration     |

[ENTRYPOINT_SCOPE]: registration — `skimage.registration`

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [CAPABILITY]                                       |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `phase_cross_correlation(reference_image, moving_image, …)` | registration   | sub-pixel image registration via phase correlation |
|  [02]   | `optical_flow_ilk(reference_image, moving_image, …)`        | optical flow   | iterative Lucas-Kanade optical flow                |
|  [03]   | `optical_flow_tvl1(reference_image, moving_image, …)`       | optical flow   | TV-L1 optical flow                                 |

[ENTRYPOINT_SCOPE]: quality metrics — `skimage.metrics`

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `structural_similarity(im1, im2, *, win_size, data_range, …)` | similarity     | SSIM structural similarity index                     |
|  [02]   | `peak_signal_noise_ratio(image_true, image_test, …)`          | quality        | PSNR in dB                                           |
|  [03]   | `hausdorff_distance(image0, image1, method)`                  | distance       | Hausdorff distance between binary images             |
|  [04]   | `mean_squared_error(image0, image1)`                          | quality        | mean squared error                                   |
|  [05]   | `normalized_root_mse(image_true, image_test, …)`              | quality        | normalized root mean squared error                   |
|  [06]   | `normalized_mutual_information(image0, image1, *, bins)`      | similarity     | normalized mutual information                        |
|  [07]   | `adapted_rand_error(image_true, image_test, *, table, ...)`   | segment metric | adapted Rand error (precision/recall) for label maps |
|  [08]   | `variation_of_information(image0, image1, *, table, ...)`     | segment metric | conditional-entropy split/merge distance             |
|  [09]   | `contingency_table(im_true, im_test, *, ignore_labels, …)`    | segment metric | sparse overlap count matrix between two label maps   |

[ENTRYPOINT_SCOPE]: region adjacency graph and path routing — `skimage.graph`

`skimage.graph` owns region-adjacency-graph (RAG) merging over a label image and minimum-cost-path routing.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :----------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `RAG(label_image, connectivity, data, ...)`                  | rag graph      | networkx-subclass region adjacency graph             |
|  [02]   | `rag_mean_color(image, labels, connectivity=2, …)`           | rag build      | RAG weighted by mean-color difference                |
|  [03]   | `rag_boundary(labels, edge_map, connectivity)`               | rag build      | RAG weighted by an edge/boundary map                 |
|  [04]   | `cut_threshold(labels, rag, thresh, in_place)`               | rag cut        | merge RAG regions below a weight threshold           |
|  [05]   | `cut_normalized(labels, rag, thresh, num_cuts, ...)`         | rag cut        | normalized-cut recursive RAG partition               |
|  [06]   | `merge_hierarchical(labels, rag, thresh, …)`                 | rag merge      | agglomerative RAG merge with merge callback          |
|  [07]   | `route_through_array(array, start, end, fully_connected, …)` | path           | minimum-cost path through a cost array               |
|  [08]   | `MCP`/`MCP_Geometric`/`MCP_Connect`/`MCP_Flexible`           | path           | minimum-cost-path finders (`find_costs`/`traceback`) |
|  [09]   | `pixel_graph(image, *, mask, edge_function, …)`              | graph build    | sparse adjacency graph of pixels with edge weights   |
|  [10]   | `central_pixel(graph, nodes, shape, partition_size)`         | graph query    | the pixel minimizing summed shortest-path distance   |

[ENTRYPOINT_SCOPE]: trainable segmentation — `skimage.future`

Classical trainable pixel segmentation: extract a multiscale feature stack, fit any sklearn-style classifier on sparse user labels, predict a full label map.

| [INDEX] | [SURFACE]                                                         | [ENTRY_FAMILY] | [CAPABILITY]                                      |
| :-----: | :---------------------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `fit_segmenter(labels, features, clf)`                            | train          | fit a classifier on labeled feature pixels        |
|  [02]   | `predict_segmenter(features, clf)`                                | predict        | predict a label map from a feature stack          |
|  [03]   | `TrainableSegmenter(clf, features_func)`                          | segmenter      | bundled fit/predict pixel segmenter               |
|  [04]   | `feature.multiscale_basic_features(image, intensity, edges, …)`   | feature stack  | the standard intensity/edge/texture feature input |
|  [05]   | `manual_lasso_segmentation(…)` / `manual_polygon_segmentation(…)` | seed label     | interactive lasso/polygon label-mask (matplotlib) |

[ENTRYPOINT_SCOPE]: utility — `skimage.util`

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `img_as_float(image, force_copy)`                              | dtype convert  | convert any image dtype to float64                   |
|  [02]   | `img_as_float32(image, force_copy)`                            | dtype convert  | convert to float32                                   |
|  [03]   | `img_as_ubyte(image, force_copy)`                              | dtype convert  | convert to uint8 with clip                           |
|  [04]   | `img_as_uint(image, force_copy)`                               | dtype convert  | convert to uint16                                    |
|  [05]   | `img_as_bool(image, force_copy)`                               | dtype convert  | convert to boolean                                   |
|  [06]   | `random_noise(image, mode, rng, clip, **kwargs)`               | augment        | add noise (Gaussian/Poisson/salt/pepper/speckle)     |
|  [07]   | `view_as_windows(arr_in, window_shape, step)`                  | windowing      | rolling window view without copy                     |
|  [08]   | `view_as_blocks(arr_in, block_shape)`                          | block view     | non-overlapping block view without copy              |
|  [09]   | `montage(arr_in, fill, rescale_intensity, grid_shape, …)`      | tiling         | tile an image stack into one montage array           |
|  [10]   | `crop(ar, …)` / `invert(image, …)` / `map_array(input_arr, …)` | array op       | symmetric crop; invert; vectorized label/value remap |
|  [11]   | `apply_parallel(function, array, chunks, depth, …)`            | parallel       | dask-chunked tiled parallel apply of a per-tile func |
|  [12]   | `regular_grid(ar_shape, n_points)`                             | sampling       | even N-point grid of slice objects over an array     |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- every function accepts and returns `numpy.ndarray`; no private image class exists, so the rail composes by passing arrays.
- dtype axis: `img_as_float`/`img_as_ubyte`/`img_as_uint`/`img_as_float32` own dtype normalization at the boundary; domain functions assume `[0,1]` float or the native integer range and recast only under `preserve_range=False`. Convert dtype explicitly.
- channel axis: multichannel functions take `channel_axis` (integer or `-1`, default `None` = grayscale); state it, never infer from shape.
- structuring elements: `footprint_rectangle(shape, *, decomposition)` and the shape factory (`disk`/`diamond`/`ball`/`ellipse`/`octagon`/`star`) own footprint construction; `decomposition='sequence'` builds a separable footprint. Never hand-roll footprint arrays.
- transform model: `estimate_transform(ttype, src, dst)` or `Model.from_estimate(src, dst)` fits from correspondences; `warp(image, model.inverse)` applies the inverse map. Models compose via `+` (matrix product) and invert via `.inverse`.
- RANSAC: `ransac(data, ModelClass, min_samples, residual_threshold, *, max_trials=100, rng=None)` returns `(model, inliers)`; the fitted `model` exposes `params` (`CircleModel`=`(xc, yc, r)`, `EllipseModel`=`(xc, yc, a, b, theta)`, `LineModelND`=`(origin, direction)`) and `residuals(data)`; the `rng` kwarg seeds it with a `numpy.random.Generator` or int. A custom model is any object with `residuals` and the `from_estimate(data)` classmethod.
- feature pipeline: `SIFT`/`ORB` detect-and-describe in one call — `detector.detect_and_extract(image)` fills `detector.keypoints`/`detector.descriptors`, then `match_descriptors(d1, d2, cross_check=True)`; `CENSURE`+`BRIEF` split it — `CENSURE().detect(image)` yields `.keypoints` only, then `BRIEF().extract(image, keypoints)` yields `.descriptors`, a detect-then-describe two-stage seam. Descriptor classes carry no array on construction.
- GLCM texture: `graycomatrix(image, distances, angles, levels, symmetric, normed)` requires an integer image (`img_as_ubyte`) and rejects floats; `graycoprops(P, prop)` reads a Haralick scalar for `prop` in `contrast`/`dissimilarity`/`homogeneity`/`ASM`/`energy`/`correlation`/`mean`/`variance`/`std`/`entropy`.
- scan-line: `measure.profile_line(image, src, dst, linewidth=1, *, reduce_func=<mean>)` samples the intensity profile along the `src`->`dst` segment (`(row, col)` pairs), returning the 1-D profile — a one-image `measure` reduction distinct from the `metrics` operand pair.
- segmentation pipeline: `label = segmenter(...)` -> `regionprops_table(label, intensity_image, properties=(...))` -> table; `rag_mean_color(image, label)` -> `merge_hierarchical`/`cut_threshold` refines regions.
- region morphometry: `regionprops`/`regionprops_table` scalar properties include `area`/`eccentricity`/`solidity`/`orientation`/`perimeter`/`euler_number`/`extent`/`axis_major_length`/`axis_minor_length`/`equivalent_diameter_area`; the array property `moments_hu` (7 rotation/scale-invariant Hu moments) expands to `moments_hu-0`…`moments_hu-6` columns, folded by a `key.startswith("moments_hu")` prefix match.
- no-reference quality: `measure.blur_effect(image, h_size=11, channel_axis=None, reduce_func=<amax>)` is the reference-free perceptual metric (re-blur strength, 0=sharp … 1=blurred), a one-image `measure` scalar distinct from the `metrics` operand-pair family.
- metrics: metric functions expect same-dtype, same-shape arrays and take `data_range` explicitly for float images.

[STACKING]:
- `pillow`(`.api/pillow.md`): the raster decode/encode owner feeding the array-transform middle — `PIL.Image.open -> numpy.asarray -> skimage.<op> -> PIL.Image.fromarray -> save`.
- `tifffile`(`.api/tifffile.md`): the scientific-format (TIFF/OME) decode owner supplying the ndarray for the same middle.
- `matplotlib`(`.api/matplotlib.md`): the overlay/label-map render tier beneath which skimage sits; `color.label2rgb` colorizes a label mask before the render.
- `resvg-py`(`.api/resvg-py.md`): rasterizes SVG/vector figures; skimage never renders in-package.
- `colour-science`(`.api/colour-science.md`): the CIE/spectral appearance owner — `color.separate_stains`/`combine_stains` unmix histological stain planes, `colour-science` owns the illuminant/appearance math.
- `numpy`(`../.api/numpy.md`): the ndarray substrate skimage builds on through `scipy.ndimage`; `scipy.sparse` arrays pass only where a function documents acceptance (e.g. `random_walker`).
- within-lib: `measure.marching_cubes` + `mesh_surface_area` extract the array isosurface and hand `verts`/`faces` to the geometry `trimesh` mesh owner for triangulation and watertight repair; `feature.multiscale_basic_features` + `future.fit_segmenter`/`predict_segmenter` feed a `scikit-learn` classifier (gated) for trainable segmentation; `util.apply_parallel` chunks a per-tile function over a `dask` array for out-of-core processing.

[LOCAL_ADMISSION]:
- `skimage.io.imread` returns an ndarray and retains no file handle; route decode through the `pillow` owner and pass the array in, reserving `skimage.io` for the `ImageCollection`/`MultiImage` lazy-glob case.
- `regionprops_table` returns a dict of equal-length arrays; hand it directly to `polars`/`dataframely` (or `great-tables` for display) at the boundary rather than looping `regionprops` objects.
- `find_contours` returns a list of `(N, 2)` float arrays in `(row, col)` order; convert to `(x, y)` at the boundary.
- `marching_cubes` returns `(verts, faces, normals, values)`; the `trimesh` mesh owner consumes `verts`/`faces` and `mesh_surface_area(verts, faces)` cross-checks area.
- denoising preserves image dtype under `preserve_range=True`; call `estimate_sigma(image, channel_axis=...)` first and feed the result to `denoise_*`/`calibrate_denoiser`.

[RAIL_LAW]:
- Package: `scikit-image`
- Owns: array-level image processing — every skimage domain operation as a pure `numpy.ndarray` transform.
- Accept: NumPy ndarray inputs; SciPy sparse arrays where documented; sklearn-style classifiers for the trainable segmenter; a dask array for `util.apply_parallel`.
- Reject: OpenCV, PIL, or hand-rolled reimplementations of operations skimage owns; per-pixel Python loops where a vectorized operation applies; an open-coded tile loop where `util.apply_parallel` owns dask-chunked parallelism; in-package rendering owned by `matplotlib`/`resvg-py`/`color.label2rgb`; image decode/encode owned by `pillow`/`tifffile`.
