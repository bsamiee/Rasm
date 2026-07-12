# [PY_ARTIFACTS_API_SCIKIT_IMAGE]

`scikit-image` (`skimage`) supplies a NumPy-array image processing pipeline for the artifacts imaging rail across thirteen domain submodules: `color` owns colorspace conversion and stain separation; `transform` owns geometric transforms, pyramids, and Hough/Radon operations; `filters` owns edge, frequency, ridge, and threshold operations; `morphology` owns structuring-element and grayscale-reconstruction operations; `segmentation` owns region/contour/level-set segmentation; `measure` owns region properties, contours, isosurface extraction, and RANSAC fitting; `feature` owns descriptor extraction, keypoint detection, texture, and blob finding; `exposure` owns histogram operations and intensity rescaling; `restoration` owns denoising, inpainting, deconvolution, and phase unwrapping; `registration` owns optical flow and phase correlation; `metrics` owns image quality and similarity measures; `graph` owns region adjacency graphs and minimum-cost-path routing; `future` owns the trainable pixel segmenter. Every operation is a pure function over `numpy.ndarray` (no private image class), so the rail is a NumPy-array transform pipeline, not an object model.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `scikit-image`
- package: `scikit-image`
- import: `skimage`
- owner: `artifacts`
- rail: imaging
- license: BSD-3-Clause (with BSD-2-Clause and MIT vendored components)
- installed: `0.26.0`
- entry points: none (library only); submodules are lazy-loaded (`lazy_loader`), so `import skimage` is cheap and each domain submodule materializes on first attribute access

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transform family
- rail: imaging — `skimage.transform`
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
- rail: imaging — `skimage.measure`
- type-family: RANSAC model

| [INDEX] | [SYMBOL]       | [CAPABILITY]                                                                                                                           |
| :-----: | :------------- | :------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `CircleModel`  | circle fit; `params=(xc, yc, r)`, `residuals(data)` per-point radial distance, `predict_xy(t)`, 0.26 `from_estimate(data)` classmethod |
|  [02]   | `EllipseModel` | ellipse fit; `params=(xc, yc, a, b, theta)`, `residuals(data)`, `predict_xy(t)`, `from_estimate(data)`                                 |
|  [03]   | `LineModelND`  | N-D line fit; `params=(origin, direction)`, `residuals(data)`, `predict_x`/`predict_y`, `from_estimate(data)`                          |

[PUBLIC_TYPE_SCOPE]: feature descriptor family
- rail: imaging — `skimage.feature`

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY]       | [CAPABILITY]                                            |
| :-----: | :-------- | :------------------ | :------------------------------------------------------ |
|  [01]   | `SIFT`    | keypoint descriptor | scale-invariant feature transform; `detect_and_extract` |
|  [02]   | `ORB`     | keypoint descriptor | oriented FAST and rotated BRIEF                         |
|  [03]   | `BRIEF`   | binary descriptor   | binary robust independent elementary features           |
|  [04]   | `CENSURE` | keypoint detector   | center-surround extremas detector                       |
|  [05]   | `Cascade` | object detector     | Haar-like feature cascade classifier                    |

[PUBLIC_TYPE_SCOPE]: I/O collection family
- rail: imaging — `skimage.io`
- type-family: lazy collection

| [INDEX] | [SYMBOL]          | [CAPABILITY]                                  |
| :-----: | :---------------- | :-------------------------------------------- |
|  [01]   | `ImageCollection` | load-on-demand sequence of images from a glob |
|  [02]   | `MultiImage`      | multi-frame image file (GIF/TIFF) collection  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: I/O
- rail: imaging — `skimage.io`

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [CAPABILITY]                            |
| :-----: | :--------------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `imread(fname, as_gray=False, **plugin_args)`                    | read           | read image file to ndarray              |
|  [02]   | `imsave(fname, arr, *, check_contrast=True, **plugin_args)`      | write          | write ndarray to image file             |
|  [03]   | `ImageCollection(load_pattern, conserve_memory, load_func, ...)` | collection     | lazy-load a glob pattern of image files |

[ENTRYPOINT_SCOPE]: color conversion
- rail: imaging — `skimage.color`

| [INDEX] | [SURFACE]                                                                                                     | [ENTRY_FAMILY]     | [CAPABILITY]                                                                               |
| :-----: | :------------------------------------------------------------------------------------------------------------ | :----------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `rgb2gray(rgb)`                                                                                               | colorspace convert | RGB to luminance float                                                                     |
|  [02]   | `rgb2hsv(rgb)`                                                                                                | colorspace convert | RGB to HSV                                                                                 |
|  [03]   | `rgb2lab(rgb, illuminant, observer, channel_axis)`                                                            | colorspace convert | RGB to CIE Lab                                                                             |
|  [04]   | `lab2rgb(lab, illuminant, observer)`                                                                          | colorspace convert | CIE Lab to RGB                                                                             |
|  [05]   | `gray2rgb(image, channel_axis)`                                                                               | colorspace convert | grayscale to 3-channel RGB                                                                 |
|  [06]   | `label2rgb(label, image, colors, alpha, ...)`                                                                 | label colorize     | map integer label mask to RGB overlay                                                      |
|  [07]   | `convert_colorspace(arr, fromspace, tospace)`                                                                 | generic convert    | dispatch colorspace conversion by name string                                              |
|  [08]   | `deltaE_ciede2000(lab1, lab2, kL, kC, kH)`                                                                    | color distance     | CIEDE2000 perceptual color difference                                                      |
|  [09]   | `separate_stains(rgb, conv_matrix, *, channel_axis)` / `combine_stains(stains, conv_matrix, *, channel_axis)` | stain separation   | color-deconvolution stain unmixing/remixing (H&E/HDX) — the `color` stain-separation owner |
|  [10]   | `rgb2ycbcr(rgb)` / `ycbcr2rgb(ycbcr)`                                                                         | colorspace convert | RGB <-> YCbCr (the broadcast/video colorspace)                                             |

[ENTRYPOINT_SCOPE]: geometric transform
- rail: imaging — `skimage.transform`

| [INDEX] | [SURFACE]                                                                                                                                                                          | [ENTRY_FAMILY]  | [CAPABILITY]                                                                     |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :-------------- | :------------------------------------------------------------------------------- |
|  [01]   | `resize(image, output_shape, order, mode, cval, clip, preserve_range, ...)`                                                                                                        | pixel resize    | resize to target shape with anti-aliasing control                                |
|  [02]   | `rescale(image, scale, order, mode, ..., channel_axis)`                                                                                                                            | scale resize    | scale by float factor per-axis                                                   |
|  [03]   | `rotate(image, angle, resize, center, order, mode, ...)`                                                                                                                           | rotation        | rotate image by degrees                                                          |
|  [04]   | `warp(image, inverse_map, map_args, output_shape, order, mode, ...)`                                                                                                               | generic warp    | apply arbitrary inverse coordinate map                                           |
|  [05]   | `AffineTransform(matrix, *, scale, shear, rotation, translation, dimensionality)`                                                                                                  | model construct | build affine transform from parameters                                           |
|  [06]   | `estimate_transform(ttype, src, dst, **kwargs)`                                                                                                                                    | model estimate  | fit a named transform type to point correspondences                              |
|  [07]   | `hough_line(image, theta)`                                                                                                                                                         | Hough           | accumulate Hough line votes                                                      |
|  [08]   | `hough_circle(image, radius, normalize, full_output)`                                                                                                                              | Hough           | Hough circle accumulation                                                        |
|  [09]   | `radon(image, theta, circle, preserve_range)`                                                                                                                                      | Radon           | compute Radon transform (sinogram)                                               |
|  [10]   | `iradon(radon_image, theta, output_size, filter_name, interpolation, circle, ...)`                                                                                                 | Radon inverse   | filtered back-projection reconstruction                                          |
|  [11]   | `hough_line_peaks(hspace, angles, dists, ...)` / `hough_circle_peaks(...)` / `probabilistic_hough_line(image, threshold, line_length, line_gap, theta, ...)`                       | Hough peaks     | extract peaks from the Hough accumulator (the detect half of the Hough pipeline) |
|  [12]   | `pyramid_gaussian(image, max_layer, downscale, ..., channel_axis)` / `pyramid_laplacian(...)`                                                                                      | pyramid         | Gaussian / Laplacian image-pyramid generators                                    |
|  [13]   | `swirl(image, center, strength, radius, ...)` / `warp_polar(image, center, *, radius, output_shape, ...)` / `integral_image(image, *, dtype)` / `matrix_transform(coords, matrix)` | warp/util       | swirl warp, log-/linear-polar unwrap, summed-area table, point-set matrix apply  |

[ENTRYPOINT_SCOPE]: filtering and thresholding
- rail: imaging — `skimage.filters`

| [INDEX] | [SURFACE]                                                                                                              | [ENTRY_FAMILY] | [CAPABILITY]                                                                   |
| :-----: | :--------------------------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `gaussian(image, sigma, *, mode, cval, preserve_range, truncate, channel_axis)`                                        | smooth         | Gaussian blur; sigma per-axis or scalar                                        |
|  [02]   | `sobel(image, mask, *, axis, mode, cval)`                                                                              | edge           | Sobel edge magnitude                                                           |
|  [03]   | `median(image, footprint, out, mode, cval, behavior)`                                                                  | smooth         | median filter with structuring element                                         |
|  [04]   | `threshold_otsu(image, nbins, *, hist)`                                                                                | threshold      | Otsu global threshold value                                                    |
|  [05]   | `threshold_local(image, block_size, method, offset, mode, ...)`                                                        | threshold      | adaptive local threshold array                                                 |
|  [06]   | `threshold_multiotsu(image, classes, nbins, *, hist)`                                                                  | threshold      | multi-class Otsu thresholds                                                    |
|  [07]   | `gabor(image, frequency, *, theta, bandwidth, sigma_x, sigma_y, ...)`                                                  | frequency      | Gabor filter real and imaginary response                                       |
|  [08]   | `frangi(image, sigmas, scale_range, ...)`                                                                              | vessel filter  | Frangi vessel-enhancement filter                                               |
|  [09]   | `unsharp_mask(image, radius, amount, multichannel, preserve_range, ...)`                                               | sharpen        | unsharp masking                                                                |
|  [10]   | `laplace(image, ksize, mask)`                                                                                          | edge           | Laplace edge detector                                                          |
|  [11]   | `butterworth(image, cutoff_frequency_ratio, high_pass, order, channel_axis, ...)`                                      | frequency      | Butterworth frequency-domain filter                                            |
|  [12]   | `scharr` / `prewitt` / `farid` / `roberts`                                                                             | edge           | alternative gradient-magnitude operators (with `_h`/`_v` directional variants) |
|  [13]   | `meijering(image, sigmas, ...)` / `sato(...)` / `hessian(...)`                                                         | ridge filter   | neuriteness/tubeness ridge filters beside `frangi`                             |
|  [14]   | `difference_of_gaussians(image, low_sigma, high_sigma, *, ..., channel_axis)`                                          | bandpass       | DoG band-pass via two Gaussians                                                |
|  [15]   | `threshold_li` / `threshold_yen` / `threshold_isodata` / `threshold_minimum` / `threshold_triangle` / `threshold_mean` | threshold      | the full global-threshold family beside Otsu                                   |
|  [16]   | `threshold_niblack(image, window_size, k)` / `threshold_sauvola(image, window_size, k, r)`                             | threshold      | local adaptive document binarization                                           |
|  [17]   | `try_all_threshold(image, figsize, verbose)`                                                                           | threshold      | apply every global threshold and return a comparison figure                    |
|  [18]   | `apply_hysteresis_threshold(image, low, high)`                                                                         | threshold      | two-level hysteresis binarization                                              |
|  [19]   | `window(window_type, shape, *, warp_kwargs)`                                                                           | frequency      | n-D apodization window for FFT pre-multiplication                              |
|  [20]   | `rank.*` (`mean`/`median`/`maximum`/`entropy`/`autolevel`/`gradient`/...)                                              | local rank     | footprint-local rank filters on uint8/uint16                                   |
|  [21]   | `wiener(image, psf, balance, ...)` / `correlate_sparse(image, kernel, mode)`                                           | deconvolve     | supervised Wiener deconvolution; sparse correlation                            |

[ENTRYPOINT_SCOPE]: morphology
- rail: imaging — `skimage.morphology`

| [INDEX] | [SURFACE]                                                                                                              | [ENTRY_FAMILY]  | [CAPABILITY]                                                                                                       |
| :-----: | :--------------------------------------------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------------------------------------------------------------------------- |
|  [01]   | `binary_erosion(image, footprint, out, *, mode)`                                                                       | binary morph    | binary erosion                                                                                                     |
|  [02]   | `binary_dilation(image, footprint, out, *, mode)`                                                                      | binary morph    | binary dilation                                                                                                    |
|  [03]   | `binary_opening(image, footprint, out, *, mode)`                                                                       | binary morph    | binary opening                                                                                                     |
|  [04]   | `binary_closing(image, footprint, out, *, mode)`                                                                       | binary morph    | binary closing                                                                                                     |
|  [05]   | `erosion(image, footprint, out, shift_x, shift_y, *, mode)`                                                            | gray morph      | grayscale erosion                                                                                                  |
|  [06]   | `dilation(image, footprint, out, shift_x, shift_y, *, mode)`                                                           | gray morph      | grayscale dilation                                                                                                 |
|  [07]   | `label(label_image, background, return_num, connectivity)`                                                             | labeling        | connected-component labeling                                                                                       |
|  [08]   | `remove_small_objects(ar, connectivity, *, min_size, out)`                                                             | filtering       | remove connected components below min_size                                                                         |
|  [09]   | `remove_small_holes(ar, area_threshold, connectivity, ...)`                                                            | filtering       | fill holes below area threshold                                                                                    |
|  [10]   | `skeletonize(image, *, method)`                                                                                        | skeleton        | topological skeleton of binary image                                                                               |
|  [11]   | `disk` / `square` / `diamond` / `ball` / `cube` / `rectangle` / `ellipse` / `octagon` / `star` / `footprint_rectangle` | structuring     | the full footprint factory (2D and 3D); `footprint_rectangle` is the 0.26 successor to `square`/`cube`/`rectangle` |
|  [12]   | `flood_fill(image, seed_point, new_value, *, footprint, ...)`                                                          | flood fill      | fill connected region with new value                                                                               |
|  [13]   | `flood(image, seed_point, *, footprint, connectivity, tolerance)`                                                      | flood           | boolean mask of the flooded region (mask form of `flood_fill`)                                                     |
|  [14]   | `white_tophat` / `black_tophat`                                                                                        | gray morph      | bright/dark feature extraction (image minus opening/closing)                                                       |
|  [15]   | `reconstruction(seed, mask, method, footprint, offset)`                                                                | gray morph      | morphological reconstruction by dilation/erosion (h-maxima, hole fill)                                             |
|  [16]   | `area_opening` / `area_closing` / `diameter_opening` / `diameter_closing`                                              | attribute morph | attribute filters removing components by area/diameter                                                             |
|  [17]   | `max_tree(image, connectivity)` / `max_tree_local_maxima(...)`                                                         | component tree  | max-tree representation for attribute filtering                                                                    |
|  [18]   | `medial_axis(image, *, mask, return_distance)` / `thin(image, max_num_iter)`                                           | skeleton        | medial-axis transform; iterative morphological thinning                                                            |
|  [19]   | `convex_hull_image(image, offset_coordinates)` / `convex_hull_object(...)`                                             | hull            | per-image / per-object 2D convex hull mask                                                                         |
|  [20]   | `isotropic_erosion` / `isotropic_dilation` / `isotropic_opening` / `isotropic_closing`                                 | binary morph    | distance-transform-based radius morphology (no explicit footprint)                                                 |

[ENTRYPOINT_SCOPE]: segmentation
- rail: imaging — `skimage.segmentation`

| [INDEX] | [SURFACE]                                                                                                                        | [ENTRY_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------- |
|  [01]   | `watershed(image, markers, connectivity, offset, mask, compactness, ...)`                                                        | region grow    | watershed from markers                                            |
|  [02]   | `slic(image, n_segments, compactness, max_num_iter, sigma, ..., channel_axis)`                                                   | superpixel     | SLIC superpixel segmentation                                      |
|  [03]   | `felzenszwalb(image, scale, sigma, min_size, *, channel_axis)`                                                                   | graph segment  | Felzenszwalb efficient graph-based segmentation                   |
|  [04]   | `active_contour(image, snake, alpha, beta, w_line, w_edge, ...)`                                                                 | contour        | active contour / snake model                                      |
|  [05]   | `chan_vese(image, mu, lambda1, lambda2, tol, max_num_iter, ...)`                                                                 | contour        | Chan-Vese level-set segmentation                                  |
|  [06]   | `random_walker(data, labels, beta, mode, tol, copy, ...)`                                                                        | probabilistic  | random walker segmentation from seed labels                       |
|  [07]   | `find_boundaries(label_img, connectivity, mode, background)`                                                                     | boundary       | boolean boundary map from label image                             |
|  [08]   | `expand_labels(label_image, distance)`                                                                                           | label expand   | expand label regions by Euclidean distance                        |
|  [09]   | `quickshift(image, ratio, kernel_size, max_dist, ..., channel_axis)`                                                             | superpixel     | quickshift mode-seeking superpixel segmentation                   |
|  [10]   | `mark_boundaries(image, label_img, color, outline_color, mode, background_label)`                                                | overlay        | draw label boundaries over an image (visualization helper)        |
|  [11]   | `clear_border(labels, buffer_size, bgval, mask, *, out)`                                                                         | label clean    | drop labels touching the image border                             |
|  [12]   | `morphological_chan_vese(...)` / `morphological_geodesic_active_contour(...)` / `inverse_gaussian_gradient(image, alpha, sigma)` | level-set      | morphological-snake level-set evolution + its edge-stopping map   |
|  [13]   | `join_segmentations(s1, s2, ...)` / `relabel_sequential(label_field, offset)`                                                    | label algebra  | intersection of two segmentations; compact consecutive relabeling |

[ENTRYPOINT_SCOPE]: region measurement
- rail: imaging — `skimage.measure`

| [INDEX] | [SURFACE]                                                                                | [ENTRY_FAMILY] | [CAPABILITY]                                                                                                                         |
| :-----: | :--------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `label(label_image, background, return_num, connectivity)`                               | label          | connected-component labeling                                                                                                         |
|  [02]   | `regionprops(label_image, intensity_image, cache, *, extra_properties, spacing, offset)` | region props   | region property list per label                                                                                                       |
|  [03]   | `regionprops_table(label_image, intensity_image, properties, *, separator, ...)`         | region table   | region properties as dict of arrays; the scalar morphometry + 7-invariant `moments_hu` property set (see [04] region morphometry)    |
|  [04]   | `find_contours(image, level, fully_connected, positive_orientation, *, mask)`            | contour        | marching-squares iso-contours                                                                                                        |
|  [05]   | `marching_cubes(volume, level, *, spacing, gradient_direction, ...)`                     | surface mesh   | 3D isosurface mesh extraction                                                                                                        |
|  [06]   | `ransac(data, model_class, min_samples, residual_threshold, ...)`                        | RANSAC         | robust fitting via RANSAC                                                                                                            |
|  [07]   | `mesh_surface_area(verts, faces)`                                                        | mesh metric    | surface area of a triangulated mesh                                                                                                  |
|  [08]   | `shannon_entropy(image, base)`                                                           | entropy        | Shannon entropy of image histogram                                                                                                   |
|  [09]   | `blur_effect(image, h_size=11, channel_axis=None, reduce_func=<amax>)`                   | no-ref quality | no-reference perceptual blur metric (0=sharp … 1=blurred), the sole reference-free `measure` scalar                                  |
|  [10]   | `profile_line(image, src, dst, linewidth=1, order, mode, cval, *, reduce_func=<mean>)`   | scan line      | intensity profile sampled along the `src`->`dst` segment; returns the 1-D profile array (the section-cut / line-profile measurement) |

[ENTRYPOINT_SCOPE]: feature detection and description
- rail: imaging — `skimage.feature`

| [INDEX] | [SURFACE]                                                                                                                                                                                                                                                               | [ENTRY_FAMILY]   | [CAPABILITY]                                                                                                                                                                                                                    |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :--------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
|  [01]   | `canny(image, sigma, low_threshold, high_threshold, mask, use_quantiles, ...)`                                                                                                                                                                                          | edge detect      | Canny multi-stage edge detector                                                                                                                                                                                                 |
|  [02]   | `hog(image, orientations=9, pixels_per_cell=(8,8), cells_per_block=(3,3), block_norm='L2-Hys', visualize=False, transform_sqrt=False, feature_vector=True, *, channel_axis=None)`                                                                                       | descriptor       | HOG feature vector; `visualize=True` returns the `(descriptor, hog_image)` two-tuple (the render is the second element)                                                                                                         |
|  [03]   | `SIFT(upsampling=2, n_octaves=8, n_scales=3, sigma_min=1.6, sigma_in=0.5, c_dog=0.0133, c_edge=10, n_bins=36, lambda_ori=1.5, c_max=0.8, lambda_descr=6, n_hist=4, n_ori=8)`                                                                                            | descriptor class | SIFT keypoint and descriptor extraction (no keypoint cap; construction carries no array)                                                                                                                                        |
|  [04]   | `match_descriptors(descriptors1, descriptors2, metric=None, p=2, max_distance=inf, cross_check=True, max_ratio=1.0)`                                                                                                                                                    | matching         | nearest-neighbor descriptor matching (Lowe-ratio + cross-check)                                                                                                                                                                 |
|  [05]   | `blob_dog(image, min_sigma, max_sigma, sigma_ratio, threshold, overlap, ...)`                                                                                                                                                                                           | blob detect      | Difference-of-Gaussian blob detection                                                                                                                                                                                           |
|  [06]   | `blob_log(image, min_sigma, max_sigma, num_sigma, threshold, overlap, ...)`                                                                                                                                                                                             | blob detect      | Laplacian-of-Gaussian blob detection                                                                                                                                                                                            |
|  [07]   | `corner_harris(image, method, k, eps, sigma, *, axis)`                                                                                                                                                                                                                  | corner detect    | Harris corner response                                                                                                                                                                                                          |
|  [08]   | `corner_peaks(image, min_distance, threshold_abs, threshold_rel, ...)`                                                                                                                                                                                                  | corner peaks     | peaks in corner response map                                                                                                                                                                                                    |
|  [09]   | `peak_local_max(image, min_distance, threshold_abs, threshold_rel, ...)`                                                                                                                                                                                                | local max        | local maxima coordinates in image                                                                                                                                                                                               |
|  [10]   | `local_binary_pattern(image, P, R, method)`                                                                                                                                                                                                                             | texture          | LBP texture descriptor                                                                                                                                                                                                          |
|  [11]   | `graycomatrix(image, distances, angles, levels=None, symmetric=False, normed=False)` / `graycoprops(P, prop='contrast')`                                                                                                                                                | texture          | GLCM + Haralick props; `prop` ∈ `contrast`/`dissimilarity`/`homogeneity`/`ASM`/`energy`/`correlation`/`mean`/`variance`/`std`/`entropy` (the last four added 0.25); British `greycomatrix`/`greycoprops` aliases removed 0.26   |
|  [12]   | `structure_tensor(image, sigma, mode, cval, *, axis, order)`                                                                                                                                                                                                            | tensor           | structure tensor elements                                                                                                                                                                                                       |
|  [13]   | `blob_doh(image, min_sigma, max_sigma, num_sigma, threshold, ...)`                                                                                                                                                                                                      | blob detect      | Determinant-of-Hessian blob detection                                                                                                                                                                                           |
|  [14]   | `ORB(downscale=1.2, n_scales=8, n_keypoints=500, fast_n=9, fast_threshold=0.08, harris_k=0.04)` / `BRIEF(descriptor_size=256, patch_size=49, mode='normal', sigma=1, ...)` / `CENSURE(min_scale=1, max_scale=7, mode='DoB', non_max_threshold=0.15, line_threshold=10)` | descriptor class | `ORB`/`SIFT` drive `detect_and_extract` -> `.keypoints`/`.descriptors`; `CENSURE.detect(image)` detects keypoints (no descriptors), `BRIEF.extract(image, keypoints)` describes given keypoints — the detect-then-describe pair |
|  [15]   | `corner_subpix` / `corner_fast` / `corner_shi_tomasi` / `corner_kitchen_rosenfeld` / `corner_moravec`                                                                                                                                                                   | corner detect    | the full corner-response family beside Harris                                                                                                                                                                                   |
|  [16]   | `hessian_matrix(image, sigma, mode, ...)` / `hessian_matrix_eigvals(H_elems)` / `shape_index(image, sigma, ...)`                                                                                                                                                        | tensor           | Hessian elements/eigenvalues and local shape index                                                                                                                                                                              |
|  [17]   | `daisy(image, step, radius, rings, ...)` / `multiscale_basic_features(...)`                                                                                                                                                                                             | descriptor       | DAISY dense descriptor; multiscale feature stack for trainable segmentation                                                                                                                                                     |
|  [18]   | `haar_like_feature(...)` / `draw_haar_like_feature(...)`                                                                                                                                                                                                                | Haar             | Haar-like feature extraction for the `Cascade` detector                                                                                                                                                                         |
|  [19]   | `fisher_vector(descriptors, gmm)` / `learn_gmm(descriptors, *, n_modes)`                                                                                                                                                                                                | aggregation      | Fisher-vector encoding over a learned GMM                                                                                                                                                                                       |

[ENTRYPOINT_SCOPE]: exposure and intensity
- rail: imaging — `skimage.exposure`

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------------ | :-------------- | :----------------------------------------------------- |
|  [01]   | `equalize_hist(image, nbins, mask)`                                 | histogram eq    | global histogram equalization                          |
|  [02]   | `equalize_adapthist(image, kernel_size, clip_limit, nbins)`         | CLAHE           | contrast-limited adaptive histogram equalization       |
|  [03]   | `rescale_intensity(image, in_range, out_range)`                     | intensity scale | linear intensity rescaling to range                    |
|  [04]   | `match_histograms(image, reference, *, channel_axis)`               | histogram match | match image histogram to reference                     |
|  [05]   | `adjust_gamma(image, gamma, gain)`                                  | gamma           | gamma correction                                       |
|  [06]   | `adjust_log(image, gain, inv)`                                      | log adjust      | logarithmic intensity adjustment                       |
|  [07]   | `histogram(image, nbins, source_range, normalize, ...)`             | histogram       | image histogram array                                  |
|  [08]   | `is_low_contrast(image, fraction_threshold, lower_percentile, ...)` | contrast check  | detect low-contrast images                             |
|  [09]   | `adjust_sigmoid(image, cutoff, gain, inv)`                          | sigmoid         | sigmoid (S-curve) contrast adjustment                  |
|  [10]   | `cumulative_distribution(image, nbins)`                             | histogram       | image CDF and bin centers (the equalization primitive) |

[ENTRYPOINT_SCOPE]: restoration and denoising
- rail: imaging — `skimage.restoration`

| [INDEX] | [SURFACE]                                                                                               | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------------------------------------------------------ | :------------- | :------------------------------------------------------ |
|  [01]   | `denoise_bilateral(image, win_size, sigma_color, sigma_spatial, ..., channel_axis)`                     | denoise        | bilateral filter; edge-preserving denoising             |
|  [02]   | `denoise_nl_means(image, patch_size, patch_distance, h, fast_mode, ..., channel_axis)`                  | denoise        | non-local means denoising                               |
|  [03]   | `denoise_wavelet(image, sigma, wavelet, mode, wavelet_levels, ..., channel_axis)`                       | denoise        | wavelet-domain denoising                                |
|  [04]   | `denoise_tv_chambolle(image, weight, eps, max_num_iter, channel_axis)`                                  | denoise        | total-variation Chambolle denoising                     |
|  [05]   | `estimate_sigma(image, average_sigmas, *, channel_axis)`                                                | noise estimate | estimate noise standard deviation                       |
|  [06]   | `inpaint_biharmonic(image, mask, *, split_into_regions, channel_axis)`                                  | inpaint        | biharmonic inpainting of masked regions                 |
|  [07]   | `richardson_lucy(image, psf, num_iter, clip, filter_epsilon, *, channel_axis)`                          | deconvolve     | Richardson-Lucy deconvolution                           |
|  [08]   | `rolling_ball(image, *, radius, kernel, nansafe, num_threads)`                                          | background     | rolling-ball background subtraction                     |
|  [09]   | `unsupervised_wiener(image, psf, reg, ..., clip, *, channel_axis)` / `wiener(image, psf, balance, ...)` | deconvolve     | self-tuned / supervised Wiener-Hunt deconvolution       |
|  [10]   | `unwrap_phase(image, wrap_around=False, rng=None)`                                                      | phase          | 2D/3D phase unwrapping (the package phase-unwrap owner) |
|  [11]   | `calibrate_denoiser(image, denoise_function, denoise_parameters, *, ...)`                               | denoise tune   | J-invariant denoiser parameter calibration              |

[ENTRYPOINT_SCOPE]: registration
- rail: imaging — `skimage.registration`

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `phase_cross_correlation(reference_image, moving_image, *, upsample_factor, space, ...)`           | registration   | sub-pixel image registration via phase correlation |
|  [02]   | `optical_flow_ilk(reference_image, moving_image, *, radius, num_warp, gaussian, prefilter, dtype)` | optical flow   | iterative Lucas-Kanade optical flow                |
|  [03]   | `optical_flow_tvl1(reference_image, moving_image, *, attachment, tightness, num_warp, ...)`        | optical flow   | TV-L1 optical flow                                 |

[ENTRYPOINT_SCOPE]: quality metrics
- rail: imaging — `skimage.metrics`

| [INDEX] | [SURFACE]                                                          | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `structural_similarity(im1, im2, *, win_size, data_range, ...)`    | similarity     | SSIM structural similarity index                        |
|  [02]   | `peak_signal_noise_ratio(image_true, image_test, *, data_range)`   | quality        | PSNR in dB                                              |
|  [03]   | `hausdorff_distance(image0, image1, method)`                       | distance       | Hausdorff distance between binary images                |
|  [04]   | `mean_squared_error(image0, image1)`                               | quality        | mean squared error                                      |
|  [05]   | `normalized_root_mse(image_true, image_test, normalization)`       | quality        | normalized root mean squared error                      |
|  [06]   | `normalized_mutual_information(image0, image1, *, bins)`           | similarity     | normalized mutual information                           |
|  [07]   | `adapted_rand_error(image_true, image_test, *, table, ...)`        | segment metric | adapted Rand error (precision/recall) for label maps    |
|  [08]   | `variation_of_information(image0, image1, *, table, ...)`          | segment metric | conditional-entropy split/merge distance for label maps |
|  [09]   | `contingency_table(im_true, im_test, *, ignore_labels, normalize)` | segment metric | sparse overlap count matrix between two label maps      |

[ENTRYPOINT_SCOPE]: region adjacency graph and path routing
- rail: imaging — `skimage.graph`

Region-adjacency-graph (RAG) merging over a label image plus minimum-cost-path routing (the latter is the `skimage.future.graph` family promoted to `skimage.graph` in release; `skimage.future.graph` no longer exists).

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------------------------- | :------------- | :------------------------------------------------------- |
|  [01]   | `RAG(label_image, connectivity, data, ...)`                              | rag graph      | networkx-subclass region adjacency graph                 |
|  [02]   | `rag_mean_color(image, labels, connectivity=2, mode='distance', sigma)`  | rag build      | RAG weighted by mean-color difference                    |
|  [03]   | `rag_boundary(labels, edge_map, connectivity)`                           | rag build      | RAG weighted by an edge/boundary map                     |
|  [04]   | `cut_threshold(labels, rag, thresh, in_place)`                           | rag cut        | merge RAG regions below a weight threshold               |
|  [05]   | `cut_normalized(labels, rag, thresh, num_cuts, ...)`                     | rag cut        | normalized-cut recursive RAG partition                   |
|  [06]   | `merge_hierarchical(labels, rag, thresh, rag_copy, in_place_merge, ...)` | rag merge      | agglomerative hierarchical RAG merge with merge callback |
|  [07]   | `route_through_array(array, start, end, fully_connected, geometric)`     | path           | minimum-cost path through a cost array                   |
|  [08]   | `MCP` / `MCP_Geometric` / `MCP_Connect` / `MCP_Flexible`                 | path           | minimum-cost-path finders (`find_costs`/`traceback`)     |
|  [09]   | `pixel_graph(image, *, mask, edge_function, connectivity, spacing)`      | graph build    | sparse adjacency graph of pixels with edge weights       |
|  [10]   | `central_pixel(graph, nodes, shape, partition_size)`                     | graph query    | the pixel minimizing summed shortest-path distance       |

[ENTRYPOINT_SCOPE]: trainable segmentation
- rail: imaging — `skimage.future`

Classical (non-deep) trainable pixel segmentation: extract a multiscale feature stack, fit any sklearn-style classifier on sparse user labels, predict a full label map.

| [INDEX] | [SURFACE]                                                                                                       | [ENTRY_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :-------------------------------------------------------------------------------------------------------------- | :------------- | :-------------------------------------------------------------- |
|  [01]   | `fit_segmenter(labels, features, clf)`                                                                          | train          | fit a classifier on labeled feature pixels                      |
|  [02]   | `predict_segmenter(features, clf)`                                                                              | predict        | predict a label map from a feature stack                        |
|  [03]   | `TrainableSegmenter(clf, features_func)`                                                                        | segmenter      | bundled fit/predict pixel segmenter                             |
|  [04]   | `feature.multiscale_basic_features(image, intensity, edges, texture, sigma_min, ...)`                           | feature stack  | the standard intensity/edge/texture feature input               |
|  [05]   | `manual_lasso_segmentation(image, alpha, return_all)` / `manual_polygon_segmentation(image, alpha, return_all)` | seed label     | interactive lasso/polygon label-mask collection (matplotlib UI) |

[ENTRYPOINT_SCOPE]: utility
- rail: imaging — `skimage.util`

| [INDEX] | [SURFACE]                                                                                                                | [ENTRY_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :----------------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `img_as_float(image, force_copy)`                                                                                        | dtype convert  | convert any image dtype to float64                             |
|  [02]   | `img_as_float32(image, force_copy)`                                                                                      | dtype convert  | convert to float32                                             |
|  [03]   | `img_as_ubyte(image, force_copy)`                                                                                        | dtype convert  | convert to uint8 with clip                                     |
|  [04]   | `img_as_uint(image, force_copy)`                                                                                         | dtype convert  | convert to uint16                                              |
|  [05]   | `img_as_bool(image, force_copy)`                                                                                         | dtype convert  | convert to boolean                                             |
|  [06]   | `random_noise(image, mode, rng, clip, **kwargs)`                                                                         | augment        | add noise (Gaussian/Poisson/salt/pepper/speckle)               |
|  [07]   | `view_as_windows(arr_in, window_shape, step)`                                                                            | windowing      | rolling window view without copy                               |
|  [08]   | `view_as_blocks(arr_in, block_shape)`                                                                                    | block view     | non-overlapping block view without copy                        |
|  [09]   | `montage(arr_in, fill, rescale_intensity, grid_shape, padding_width, *, channel_axis)`                                   | tiling         | tile an image stack into one montage array                     |
|  [10]   | `crop(ar, crop_width, copy, order)` / `invert(image, *, signed_float)` / `map_array(input_arr, input_vals, output_vals)` | array op       | symmetric crop; intensity invert; vectorized label/value remap |
|  [11]   | `apply_parallel(function, array, chunks, depth, mode, ..., compute, *, channel_axis, dtype)`                             | parallel       | dask-chunked tiled parallel apply of any per-tile function     |
|  [12]   | `regular_grid(ar_shape, n_points)`                                                                                       | sampling       | even N-point grid of slice objects over an array               |

## [04]-[IMPLEMENTATION_LAW]

[IMAGING_TOPOLOGY]:
- all functions accept and return `numpy.ndarray`; there is no private image class, so the rail composes by passing arrays, not objects.
- dtype axis: `img_as_float`/`img_as_ubyte`/`img_as_uint`/`img_as_float32` own dtype normalization at the boundary; domain functions assume `[0,1]` float or the native integer range and do not recast internally unless `preserve_range=False`. Drive dtype conversion explicitly; never rely on implicit promotion.
- channel axis: multichannel functions take `channel_axis` (integer or `-1`, default `None` = grayscale); never infer from shape. The legacy `multichannel=` keyword is removed in 0.26.
- structuring elements: the `skimage.morphology` footprint factory (`disk`/`diamond`/`ball`/`ellipse`/`octagon`/`star` plus the unified `footprint_rectangle` that supersedes `square`/`cube`/`rectangle` — the predecessors remain callable but emit a `FutureWarning` since 0.25, so new code uses `footprint_rectangle`) and the `decomposition='sequence'` argument own structuring-element construction; never hand-roll footprint arrays.
- transform model: `estimate_transform(ttype, src, dst)` or `Model().estimate(src, dst)` fits from correspondences; `warp(image, model.inverse)` applies the inverse map. Transform objects compose via `+` (matrix product) and invert via `.inverse`.
- RANSAC: `ransac(data, ModelClass, min_samples, residual_threshold, *, max_trials=100, rng=None, ...)` wraps any model exposing `residuals` and (0.26) `from_estimate`/`estimate`; the seed kwarg is `rng` (a `numpy.random.Generator`/int), not `random_state`. It returns `(model, inliers)` where the fitted `model` exposes `params` (`CircleModel`=`(xc, yc, r)`, `EllipseModel`=`(xc, yc, a, b, theta)`, `LineModelND`=`(origin, direction)`) and `residuals(data)`. 0.26 replaces the deprecated `estimate` method with the `from_estimate(data)` classmethod and deprecates no-arg construction — but `ransac` still accepts the model class and instantiates it internally, so `ransac(points, CircleModel, ...)` is the current form; a custom model is any object with `residuals` + `from_estimate`.
- feature pipeline: `SIFT`/`ORB` are detect-and-describe — `detector = SIFT()/ORB(...); detector.detect_and_extract(image)` -> `detector.keypoints`/`detector.descriptors` -> `match_descriptors(d1, d2, cross_check=True)`; `CENSURE`/`BRIEF` split it — `CENSURE().detect(image)` -> `.keypoints` (no descriptors), then `BRIEF().extract(image, keypoints)` -> `.descriptors` — so a CENSURE+BRIEF pipeline is a detect-then-describe two-stage seam, not the uniform `detect_and_extract`. Descriptor classes carry no array on construction.
- GLCM texture: `graycoprops(P, prop)` reads a Haralick scalar from the `graycomatrix` — the valid `prop` literals are `contrast`/`dissimilarity`/`homogeneity`/`ASM`/`energy`/`correlation` plus (release) `mean`/`variance`/`std`/`entropy`; `graycomatrix(image, distances, angles, levels=None, symmetric=False, normed=False)` requires an integer image (`img_as_ubyte`) and rejects floats, `symmetric=True`+`normed=True` yielding the symmetric normalized histogram.
- scan-line: `measure.profile_line(image, src, dst, linewidth=1, *, reduce_func=<mean>)` samples the intensity profile along the `src`->`dst` segment (`src`/`dst` are `(row, col)` pairs), returning the 1-D profile array — the section-cut / line-profile measurement, a one-image `measure` reduction distinct from the `metrics` operand pair.
- segmentation pipeline: `label = segmenter(...)` -> `regionprops_table(label, intensity_image, properties=(...))` -> table; RAG refinement is `rag_mean_color(image, label) -> merge_hierarchical/cut_threshold` for region merging.
- region morphometry: `regionprops`/`regionprops_table` scalar properties are `area`/`eccentricity`/`solidity`/`orientation`/`perimeter`/`euler_number`/`extent`/`axis_major_length`/`axis_minor_length`/`equivalent_diameter_area`; the array property `moments_hu` is the 7 rotation/scale-invariant Hu moments and `regionprops_table` expands it to `moments_hu-0`…`moments_hu-6` columns (fold by a `key.startswith("moments_hu")` prefix match, never assuming the `-` separator).
- no-reference quality: `measure.blur_effect(image, h_size=11, channel_axis=None, reduce_func=<amax>)` is the only reference-free perceptual metric (re-blur strength -> 0=sharp … 1=blurred), a one-image `measure` scalar distinct from the `metrics` operand-pair quality family.
- metrics: all metric functions expect same-dtype, same-shape arrays; provide `data_range` explicitly for float images (it is no longer inferred).

[LOCAL_ADMISSION]:
- `skimage.io.imread` returns a NumPy array; the owner does not retain any file handle. Prefer routing decode through the sibling `pillow` owner and passing the resulting array in, reserving `skimage.io` for the `ImageCollection`/`MultiImage` lazy-glob case.
- `regionprops_table` returns a dict of equal-length arrays; hand it directly to `polars`/`dataframely` (or `great-tables` for a display table) at the boundary instead of looping `regionprops` objects.
- `find_contours` returns a list of `(N, 2)` float arrays in `(row, col)` order; convert to `(x, y)` at the boundary.
- `marching_cubes` returns `(verts, faces, normals, values)` (4-tuple, `method='lewiner'` default); the `trimesh` mesh owner consumes `verts`/`faces` directly and `mesh_surface_area(verts, faces)` cross-checks area.
- denoising functions preserve image dtype when `preserve_range=True`; call `estimate_sigma(image, channel_axis=...)` first and feed the result to `denoise_*`/`calibrate_denoiser` rather than guessing the noise level.

[INTEGRATION]:
- decode/encode is owned by `pillow` (raster) and `tifffile`/`imageio` (scientific formats, transitive); the standard rail is `PIL.Image.open -> numpy.asarray -> skimage.<op> -> PIL.Image.fromarray -> save`, keeping skimage strictly in the array-transform middle.
- skimage is the array-algorithm tier beneath the visualization tier: overlay/label maps render through `matplotlib` (gated sibling) or colorize via `color.label2rgb`; SVG/vector figures rasterize through `resvg_py`; never render inside skimage.
- the 3D `measure.marching_cubes` isosurface and `measure.mesh_surface_area` stack with the geometry `trimesh`/mesh owner — skimage extracts the array isosurface, the mesh owner owns the triangulation and watertight repair.
- `feature.multiscale_basic_features` + `future.fit_segmenter`/`predict_segmenter` stack with a `scikit-learn` classifier (gated sibling) for trainable segmentation; skimage owns the feature stack, sklearn owns the model fit.
- `color.separate_stains`/`combine_stains` color-deconvolution stacks with `colour-science` (the artifacts CIE/spectral owner) — skimage unmixes the histological stain planes, `colour-science` owns the appearance/illuminant math; never re-derive a stain matrix.
- `util.apply_parallel` chunks a per-tile function over a `dask` array (the data-tier dask), so a large image processes out-of-core through the same tiling the data rail already owns — never an open-coded tile loop.
- numeric heavy lifting is `numpy`/`scipy` (skimage builds on `scipy.ndimage`); pass `scipy.sparse` arrays only where a function documents acceptance (e.g. `random_walker`).

[RAIL_LAW]:
- Package: `scikit-image`
- Owns: array-level image processing across color/stain-separation, geometry and pyramids and Hough/Radon, filtering/ridge/threshold, morphology and reconstruction, segmentation and level-sets/morphological-snakes, measurement and isosurface extraction, feature/texture/descriptor extraction, exposure, denoising/deconvolution/inpainting/phase-unwrapping, registration/optical-flow, quality metrics, region adjacency graphs, and trainable pixel segmentation
- Accept: NumPy ndarray inputs; SciPy sparse arrays where documented; sklearn-style classifiers for the trainable segmenter; a dask array for `util.apply_parallel`
- Reject: OpenCV, PIL, or hand-rolled reimplementations of operations skimage already owns; per-pixel Python loops where a vectorized skimage operation applies; an open-coded tile loop where `util.apply_parallel` owns dask-chunked parallelism; in-package rendering where `matplotlib`/`resvg_py`/`color.label2rgb` own visualization; image decode/encode where `pillow`/`tifffile` own the codec
