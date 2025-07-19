# Continuous Wavelet Transform Analysis of Bearing Vibrations

## Overview

This repository provides a program for **analyzing mechanical bearing vibrations** using the **Continuous Wavelet Transform (CWT)**.  
The software enables comparison of the classic Fourier Transform and wavelet analysis for both synthetic and real (experimental) signals, with a user-friendly graphical interface (C#, Windows).

The approach allows detailed investigation of high- and low-frequency signal components, as well as temporal localization of features—critical for identifying the onset and progression of critical loads in mechanical systems.

## Features

- **Import signals** from `.txt`, `.csv`, `.mp3` files or mathematical expressions.
- **Visualization:**
  - Raw signal graph (amplitude vs time)
  - Fourier spectrum (frequency domain)
  - Wavelet spectrogram (time-frequency domain)
- **Comparison** with MATLAB and Python implementations (pywavelets)
- **Real-world analysis:** Example datasets from [PRONOSTIA](https://ieeexplore.ieee.org/document/6238642)
- **Graphical User Interface:** Simple signal selection, processing options, and visualization

## Theoretical Background

### Fourier Transform

The classical method for signal analysis in the frequency domain.  
**Limitation:** Loses information about *when* a frequency occurs—cannot analyze non-stationary signals.

### Wavelet Transform

An extension of spectral analysis suitable for *non-stationary* signals.  
Wavelets are localized in both time and frequency, allowing you to detect and track local events, impulses, and transient processes.

- **Implemented method:** Continuous Wavelet Transform (CWT) using the MHAT (Mexican Hat) wavelet.

## Usage Example

1. **Load a signal** (file or mathematical function)
2. **Adjust spectrogram and time parameters** in the GUI
3. **Visualize** signal, Fourier spectrum, and wavelet spectrogram
4. **Analyze** the time-frequency characteristics, find local anomalies, and compare stationary vs non-stationary processes

#### Program Main Window
<img width="2559" height="1390" alt="image" src="https://github.com/user-attachments/assets/82986350-28b9-4968-ae3a-05b9369e925c" />

#### Sample Output

| Signal              | Fourier Spectrum    | Wavelet Spectrogram   |
|---------------------|--------------------|-----------------------|
| <img width="871" height="125" alt="image" src="https://github.com/user-attachments/assets/8f4a5a8d-0103-4c42-a399-e4e93c2e0ad6" />|<img width="879" height="136" alt="image" src="https://github.com/user-attachments/assets/a424c217-4f4a-4754-a516-ff93ae9d698a" />| <img width="826" height="367" alt="image" src="https://github.com/user-attachments/assets/5a7e00bf-f591-48d0-92ea-3111eba1b02a" />|


## Installation

**Requirements:**
- Windows OS
- .NET Framework 4.7+  
- Visual Studio 2019+ (or compatible)

**Steps:**

```sh
git clone https://github.com/your-username/your-repo.git
