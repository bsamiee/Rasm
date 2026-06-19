# [PY_ARTIFACTS_API_SCIKIT_IMAGE]

`scikit-image` (`skimage`) supplies a NumPy-array image processing pipeline for the artifacts compute rail across eleven domain submodules: `color` owns colorspace conversion; `transform` owns geometric transforms and Hough/Radon operations; `filters` owns edge, frequency, and threshold operations; `morphology` owns structuring-element operations; `segmentation` owns region and contour segmentation; `measure` owns region properties, contours, and RANSAC fitting; `feature` owns descriptor extraction, keypoint detection, and blob finding; `exposure` owns histogram operations and intensity rescaling; `restoration` owns denoising and inpainting; `registration` owns optical flow and phase correlation; `metrics` owns image quality and similarity measures.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scikit-image`
- package: `scikit-image`
- module: `skimage`
- asset: runtime library
- rail: compute

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transform family
- rail: compute — `skimage.transform`
- type-family: geometric model

| [INDEX] | [SYMBOL]                     | [CAPABILITY]                                                |
| :-----: | :--------------------------- | :---------------------------------------------------------- |
|  [01]   | `AffineTransform`            | scale, shear, rotation, translation; `estimate` from points |
|  [02]   | `EuclideanTransform`         | rotation and translation only                               |
|  [03]   | `SimilarityTransform`        | uniform scale, rotation, translation                        |
|  [04]   | `ProjectiveTransform`        | full homography; `estimate` via DLT                         |
|  [05]   | `EssentialMatrixTransform`   | essential matrix estimation from correspondences            |
|  [06]   | `FundamentalMatrixTransform` | fundamental matrix estimation                               |
|  [07]   | `PolynomialTransform`        | polynomial warp model                                       |
|  [08]   | `PiecewiseAffineTransform`   | triangulated piecewise affine warp                          |
|  [09]   | `ThinPlateSplineTransform`   | thin-plate spline interpolation warp                        |

[PUBLIC_TYPE_SCOPE]: measure fitting family
- rail: compute — `skimage.measure`
- type-family: RANSAC model

| [INDEX] | [SYMBOL]       | [CAPABILITY]                               |
| :-----: | :------------- | :----------------------------------------- |
|  [01]   | `CircleModel`  | circle fitting from point correspondences  |
|  [02]   | `EllipseModel` | ellipse fitting from point correspondences |
|  [03]   | `LineModelND`  | N-dimensional line fitting                 |

[PUBLIC_TYPE_SCOPE]: feature descriptor family
- rail: compute — `skimage.feature`

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY]       | [CAPABILITY]                                            |
| :-----: | :-------- | :------------------ | :------------------------------------------------------ |
|  [01]   | `SIFT`    | keypoint descriptor | scale-invariant feature transform; `detect_and_extract` |
|  [02]   | `ORB`     | keypoint descriptor | oriented FAST and rotated BRIEF                         |
|  [03]   | `BRIEF`   | binary descriptor   | binary robust independent elementary features           |
|  [04]   | `CENSURE` | keypoint detector   | center-surround extremas detector                       |
|  [05]   | `Cascade` | object detector     | Haar-like feature cascade classifier                    |

[PUBLIC_TYPE_SCOPE]: I/O collection family
- rail: compute — `skimage.io`
- type-family: lazy collection

| [INDEX] | [SYMBOL]          | [CAPABILITY]                                  |
| :-----: | :---------------- | :-------------------------------------------- |
|  [01]   | `ImageCollection` | load-on-demand sequence of images from a glob |
|  [02]   | `MultiImage`      | multi-frame image file (GIF/TIFF) collection  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: I/O
- rail: compute — `skimage.io`

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `imread(fname, as_gray=False, **plugin_args)`                    | read           | read image file to ndarray              |
|  [02]   | `imsave(fname, arr, *, check_contrast=True, **plugin_args)`      | write          | write ndarray to image file             |
|  [03]   | `ImageCollection(load_pattern, conserve_memory, load_func, ...)` | collection     | lazy-load a glob pattern of image files |

[ENTRYPOINT_SCOPE]: color conversion
- rail: compute — `skimage.color`

| [INDEX] | [SURFACE]                                          | [ENTRY_FAMILY]     | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------- | :----------------- | :-------------------------------------------- |
|  [01]   | `rgb2gray(rgb)`                                    | colorspace convert | RGB to luminance float                        |
|  [02]   | `rgb2hsv(rgb)`                                     | colorspace convert | RGB to HSV                                    |
|  [03]   | `rgb2lab(rgb, illuminant, observer, channel_axis)` | colorspace convert | RGB to CIE Lab                                |
|  [04]   | `lab2rgb(lab, illuminant, observer)`               | colorspace convert | CIE Lab to RGB                                |
|  [05]   | `gray2rgb(image, channel_axis)`                    | colorspace convert | grayscale to 3-channel RGB                    |
|  [06]   | `label2rgb(label, image, colors, alpha, ...)`      | label colorize     | map integer label mask to RGB overlay         |
|  [07]   | `convert_colorspace(arr, fromspace, tospace)`      | generic convert    | dispatch colorspace conversion by name string |
|  [08]   | `deltaE_ciede2000(lab1, lab2, kL, kC, kH)`         | color distance     | CIEDE2000 perceptual color difference         |

[ENTRYPOINT_SCOPE]: geometric transform
- rail: compute — `skimage.transform`

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY]  | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------------------------------------- | :-------------- | :-------------------------------------------------- |
|  [01]   | `resize(image, output_shape, order, mode, cval, clip, preserve_range, ...)`        | pixel resize    | resize to target shape with anti-aliasing control   |
|  [02]   | `rescale(image, scale, order, mode, ..., channel_axis)`                            | scale resize    | scale by float factor per-axis                      |
|  [03]   | `rotate(image, angle, resize, center, order, mode, ...)`                           | rotation        | rotate image by degrees                             |
|  [04]   | `warp(image, inverse_map, map_args, output_shape, order, mode, ...)`               | generic warp    | apply arbitrary inverse coordinate map              |
|  [05]   | `AffineTransform(matrix, *, scale, shear, rotation, translation, dimensionality)`  | model construct | build affine transform from parameters              |
|  [06]   | `estimate_transform(ttype, src, dst, **kwargs)`                                    | model estimate  | fit a named transform type to point correspondences |
|  [07]   | `hough_line(image, theta)`                                                         | Hough           | accumulate Hough line votes                         |
|  [08]   | `hough_circle(image, radius, normalize, full_output)`                              | Hough           | Hough circle accumulation                           |
|  [09]   | `radon(image, theta, circle, preserve_range)`                                      | Radon           | compute Radon transform (sinogram)                  |
|  [10]   | `iradon(radon_image, theta, output_size, filter_name, interpolation, circle, ...)` | Radon inverse   | filtered back-projection reconstruction             |

[ENTRYPOINT_SCOPE]: filtering and thresholding
- rail: compute — `skimage.filters`

| [INDEX] | [SURFACE]                                                                         | [ENTRY_FAMILY] | [CAPABILITY]                             |
| :-----: | :-------------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `gaussian(image, sigma, *, mode, cval, preserve_range, truncate, channel_axis)`   | smooth         | Gaussian blur; sigma per-axis or scalar  |
|  [02]   | `sobel(image, mask, *, axis, mode, cval)`                                         | edge           | Sobel edge magnitude                     |
|  [03]   | `median(image, footprint, out, mode, cval, behavior)`                             | smooth         | median filter with structuring element   |
|  [04]   | `threshold_otsu(image, nbins, *, hist)`                                           | threshold      | Otsu global threshold value              |
|  [05]   | `threshold_local(image, block_size, method, offset, mode, ...)`                   | threshold      | adaptive local threshold array           |
|  [06]   | `threshold_multiotsu(image, classes, nbins, *, hist)`                             | threshold      | multi-class Otsu thresholds              |
|  [07]   | `gabor(image, frequency, *, theta, bandwidth, sigma_x, sigma_y, ...)`             | frequency      | Gabor filter real and imaginary response |
|  [08]   | `frangi(image, sigmas, scale_range, ...)`                                         | vessel filter  | Frangi vessel-enhancement filter         |
|  [09]   | `unsharp_mask(image, radius, amount, multichannel, preserve_range, ...)`          | sharpen        | unsharp masking                          |
|  [10]   | `laplace(image, ksize, mask)`                                                     | edge           | Laplace edge detector                    |
|  [11]   | `butterworth(image, cutoff_frequency_ratio, high_pass, order, channel_axis, ...)` | frequency      | Butterworth frequency-domain filter      |

[ENTRYPOINT_SCOPE]: morphology
- rail: compute — `skimage.morphology`

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------ | :------------- | :----------------------------------------- |
|  [01]   | `binary_erosion(image, footprint, out, *, mode)`              | binary morph   | binary erosion                             |
|  [02]   | `binary_dilation(image, footprint, out, *, mode)`             | binary morph   | binary dilation                            |
|  [03]   | `binary_opening(image, footprint, out, *, mode)`              | binary morph   | binary opening                             |
|  [04]   | `binary_closing(image, footprint, out, *, mode)`              | binary morph   | binary closing                             |
|  [05]   | `erosion(image, footprint, out, shift_x, shift_y, *, mode)`   | gray morph     | grayscale erosion                          |
|  [06]   | `dilation(image, footprint, out, shift_x, shift_y, *, mode)`  | gray morph     | grayscale dilation                         |
|  [07]   | `label(label_image, background, return_num, connectivity)`    | labeling       | connected-component labeling               |
|  [08]   | `remove_small_objects(ar, connectivity, *, max_size, out)`    | filtering      | remove connected components below max_size |
|  [09]   | `remove_small_holes(ar, area_threshold, connectivity, ...)`   | filtering      | fill holes below area threshold            |
|  [10]   | `skeletonize(image, *, method)`                               | skeleton       | topological skeleton of binary image       |
|  [11]   | `disk(radius, dtype, *, strict_radius, decomposition)`        | structuring    | circular structuring element               |
|  [12]   | `flood_fill(image, seed_point, new_value, *, footprint, ...)` | flood fill     | fill connected region with new value       |

[ENTRYPOINT_SCOPE]: segmentation
- rail: compute — `skimage.segmentation`

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY] | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `watershed(image, markers, connectivity, offset, mask, compactness, ...)`      | region grow    | watershed from markers                          |
|  [02]   | `slic(image, n_segments, compactness, max_num_iter, sigma, ..., channel_axis)` | superpixel     | SLIC superpixel segmentation                    |
|  [03]   | `felzenszwalb(image, scale, sigma, min_size, *, channel_axis)`                 | graph segment  | Felzenszwalb efficient graph-based segmentation |
|  [04]   | `active_contour(image, snake, alpha, beta, w_line, w_edge, ...)`               | contour        | active contour / snake model                    |
|  [05]   | `chan_vese(image, mu, lambda1, lambda2, tol, max_num_iter, ...)`               | contour        | Chan-Vese level-set segmentation                |
|  [06]   | `random_walker(data, labels, beta, mode, tol, copy, ...)`                      | probabilistic  | random walker segmentation from seed labels     |
|  [07]   | `find_boundaries(label_img, connectivity, mode, background)`                   | boundary       | boolean boundary map from label image           |
|  [08]   | `expand_labels(label_image, distance)`                                         | label expand   | expand label regions by Euclidean distance      |

[ENTRYPOINT_SCOPE]: region measurement
- rail: compute — `skimage.measure`

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `label(label_image, background, return_num, connectivity)`                               | label          | connected-component labeling                          |
|  [02]   | `regionprops(label_image, intensity_image, cache, *, extra_properties, spacing, offset)` | region props   | region property list per label                        |
|  [03]   | `regionprops_table(label_image, intensity_image, properties, *, separator, ...)`         | region table   | region properties as dict of arrays (DataFrame-ready) |
|  [04]   | `find_contours(image, level, fully_connected, positive_orientation, *, mask)`            | contour        | marching-squares iso-contours                         |
|  [05]   | `marching_cubes(volume, level, *, spacing, gradient_direction, ...)`                     | surface mesh   | 3D isosurface mesh extraction                         |
|  [06]   | `ransac(data, model_class, min_samples, residual_threshold, ...)`                        | RANSAC         | robust fitting via RANSAC                             |
|  [07]   | `mesh_surface_area(verts, faces)`                                                        | mesh metric    | surface area of a triangulated mesh                   |
|  [08]   | `shannon_entropy(image, base)`                                                           | entropy        | Shannon entropy of image histogram                    |

[ENTRYPOINT_SCOPE]: feature detection and description
- rail: compute — `skimage.feature`

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY]   | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------------------ | :--------------- | :--------------------------------------------- |
|  [01]   | `canny(image, sigma, low_threshold, high_threshold, mask, use_quantiles, ...)`  | edge detect      | Canny multi-stage edge detector                |
|  [02]   | `hog(image, orientations, pixels_per_cell, cells_per_block, ..., channel_axis)` | descriptor       | histogram of oriented gradients feature vector |
|  [03]   | `SIFT(upsampling, n_octaves, n_scales, sigma_min, ...)`                         | descriptor class | SIFT keypoint and descriptor extraction        |
|  [04]   | `match_descriptors(descriptors1, descriptors2, metric, p, max_distance, ...)`   | matching         | nearest-neighbor descriptor matching           |
|  [05]   | `blob_dog(image, min_sigma, max_sigma, sigma_ratio, threshold, overlap, ...)`   | blob detect      | Difference-of-Gaussian blob detection          |
|  [06]   | `blob_log(image, min_sigma, max_sigma, num_sigma, threshold, overlap, ...)`     | blob detect      | Laplacian-of-Gaussian blob detection           |
|  [07]   | `corner_harris(image, method, k, eps, sigma, *, axis)`                          | corner detect    | Harris corner response                         |
|  [08]   | `corner_peaks(image, min_distance, threshold_abs, threshold_rel, ...)`          | corner peaks     | peaks in corner response map                   |
|  [09]   | `peak_local_max(image, min_distance, threshold_abs, threshold_rel, ...)`        | local max        | local maxima coordinates in image              |
|  [10]   | `local_binary_pattern(image, P, R, method)`                                     | texture          | LBP texture descriptor                         |
|  [11]   | `graycomatrix(image, distances, angles, levels, ...)`                           | texture          | gray-level co-occurrence matrix                |
|  [12]   | `structure_tensor(image, sigma, mode, cval, *, axis, order)`                    | tensor           | structure tensor elements                      |

[ENTRYPOINT_SCOPE]: exposure and intensity
- rail: compute — `skimage.exposure`

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY]  | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------ | :-------------- | :----------------------------------------------- |
|  [01]   | `equalize_hist(image, nbins, mask)`                                 | histogram eq    | global histogram equalization                    |
|  [02]   | `equalize_adapthist(image, kernel_size, clip_limit, nbins)`         | CLAHE           | contrast-limited adaptive histogram equalization |
|  [03]   | `rescale_intensity(image, in_range, out_range)`                     | intensity scale | linear intensity rescaling to range              |
|  [04]   | `match_histograms(image, reference, *, channel_axis)`               | histogram match | match image histogram to reference               |
|  [05]   | `adjust_gamma(image, gamma, gain)`                                  | gamma           | gamma correction                                 |
|  [06]   | `adjust_log(image, gain, inv)`                                      | log adjust      | logarithmic intensity adjustment                 |
|  [07]   | `histogram(image, nbins, source_range, normalize, ...)`             | histogram       | image histogram array                            |
|  [08]   | `is_low_contrast(image, fraction_threshold, lower_percentile, ...)` | contrast check  | detect low-contrast images                       |

[ENTRYPOINT_SCOPE]: restoration and denoising
- rail: compute — `skimage.restoration`

| [INDEX] | [SURFACE]                                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------------------- | :------------- | :------------------------------------------ |
|  [01]   | `denoise_bilateral(image, win_size, sigma_color, sigma_spatial, ..., channel_axis)`    | denoise        | bilateral filter; edge-preserving denoising |
|  [02]   | `denoise_nl_means(image, patch_size, patch_distance, h, fast_mode, ..., channel_axis)` | denoise        | non-local means denoising                   |
|  [03]   | `denoise_wavelet(image, sigma, wavelet, mode, wavelet_levels, ..., channel_axis)`      | denoise        | wavelet-domain denoising                    |
|  [04]   | `denoise_tv_chambolle(image, weight, eps, max_num_iter, channel_axis)`                 | denoise        | total-variation Chambolle denoising         |
|  [05]   | `estimate_sigma(image, average_sigmas, *, channel_axis)`                               | noise estimate | estimate noise standard deviation           |
|  [06]   | `inpaint_biharmonic(image, mask, *, split_into_regions, channel_axis)`                 | inpaint        | biharmonic inpainting of masked regions     |
|  [07]   | `richardson_lucy(image, psf, num_iter, clip, filter_epsilon, *, channel_axis)`         | deconvolve     | Richardson-Lucy deconvolution               |
|  [08]   | `rolling_ball(image, *, radius, kernel, nansafe, num_threads)`                         | background     | rolling-ball background subtraction         |

[ENTRYPOINT_SCOPE]: registration
- rail: compute — `skimage.registration`

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `phase_cross_correlation(reference_image, moving_image, *, upsample_factor, space, ...)`           | registration   | sub-pixel image registration via phase correlation |
|  [02]   | `optical_flow_ilk(reference_image, moving_image, *, radius, num_warp, gaussian, prefilter, dtype)` | optical flow   | iterative Lucas-Kanade optical flow                |
|  [03]   | `optical_flow_tvl1(reference_image, moving_image, *, attachment, tightness, num_warp, ...)`        | optical flow   | TV-L1 optical flow                                 |

[ENTRYPOINT_SCOPE]: quality metrics
- rail: compute — `skimage.metrics`

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `structural_similarity(im1, im2, *, win_size, data_range, ...)`  | similarity     | SSIM structural similarity index         |
|  [02]   | `peak_signal_noise_ratio(image_true, image_test, *, data_range)` | quality        | PSNR in dB                               |
|  [03]   | `hausdorff_distance(image0, image1, method)`                     | distance       | Hausdorff distance between binary images |
|  [04]   | `mean_squared_error(image0, image1)`                             | quality        | mean squared error                       |
|  [05]   | `normalized_root_mse(image_true, image_test, normalization)`     | quality        | normalized root mean squared error       |
|  [06]   | `normalized_mutual_information(image0, image1, *, bins)`         | similarity     | normalized mutual information            |

[ENTRYPOINT_SCOPE]: utility
- rail: compute — `skimage.util`

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `img_as_float(image, force_copy)`                | dtype convert  | convert any image dtype to float64               |
|  [02]   | `img_as_float32(image, force_copy)`              | dtype convert  | convert to float32                               |
|  [03]   | `img_as_ubyte(image, force_copy)`                | dtype convert  | convert to uint8 with clip                       |
|  [04]   | `img_as_uint(image, force_copy)`                 | dtype convert  | convert to uint16                                |
|  [05]   | `img_as_bool(image, force_copy)`                 | dtype convert  | convert to boolean                               |
|  [06]   | `random_noise(image, mode, rng, clip, **kwargs)` | augment        | add noise (Gaussian/Poisson/salt/pepper/speckle) |
|  [07]   | `view_as_windows(arr_in, window_shape, step)`    | windowing      | rolling window view without copy                 |
|  [08]   | `view_as_blocks(arr_in, block_shape)`            | block view     | non-overlapping block view without copy          |

## [04]-[IMPLEMENTATION_LAW]

[COMPUTE_TOPOLOGY]:
- all functions accept NumPy ndarrays; no private image classes are required
- dtype axis: `img_as_float` / `img_as_ubyte` own dtype normalization at the boundary; domain functions do not recast internally unless `preserve_range=False`
- channel axis: multichannel functions accept `channel_axis` kwarg (integer or -1); never infer from shape
- structuring elements: `disk`, `square`, `diamond`, `ball`, `cube`, `rectangle`, `ellipse` from `skimage.morphology` are the footprint factory; do not hand-roll footprint arrays
- transform model: `estimate_transform(ttype, src, dst)` or `Model.estimate(src, dst)` fits from correspondences; `warp(image, model.inverse)` applies it
- RANSAC: `ransac(data, ModelClass, min_samples, residual_threshold)` wraps any `estimate`/`residuals` model; `CircleModel`, `EllipseModel`, `LineModelND` are the built-in models
- feature pipeline: `detector.detect_and_extract(image)` -> `detector.keypoints`, `detector.descriptors` -> `match_descriptors`
- metrics: all metric functions expect same-dtype, same-shape arrays; provide `data_range` explicitly for float images

[LOCAL_ADMISSION]:
- `skimage.io.imread` returns a NumPy array; the owner does not retain any file handle.
- `regionprops_table` returns a dict of arrays; convert to `pd.DataFrame` at the boundary when tabular downstream consumption requires it.
- `find_contours` returns a list of `(N, 2)` float arrays in `(row, col)` order; convert to `(x, y)` at the boundary.
- `marching_cubes` returns `(verts, faces, normals, values)`; the mesh owner consumes the tuple directly.
- All denoising functions preserve image dtype when `preserve_range=True` or channel content is not float; check `estimate_sigma` output before choosing denoiser parameters.

[RAIL_LAW]:
- Package: `scikit-image`
- Owns: array-level image processing across color, geometry, filtering, morphology, segmentation, measurement, feature extraction, exposure, denoising, registration, and metrics
- Accept: NumPy ndarray inputs; SciPy sparse arrays where documented
- Reject: OpenCV, PIL, or hand-rolled reimplementations of operations skimage already owns; per-pixel Python loops where vectorized skimage operations apply
