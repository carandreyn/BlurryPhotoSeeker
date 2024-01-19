# BlurryPhotoSeeker

## Overview

BlurryPhotoSeeker is a simple WPF tool for detecting and moving blurry photos from a selected folder.

## Features

- **Open Folder:** Select a folder to scan for images.
- **Blurry Detection:** Automatically identifies blurry photos using Laplacian variance.
- **Move to BlurryPhotos:** Moves detected blurry photos to a "BlurryPhotos" folder within the selected folder.

## Getting Started

### Prerequisites

- .NET Framework (>= 4.5.2)
- Emgu.CV (Install via NuGet: `Install-Package Emgu.CV -Version 4.5.3.60`)

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/your-username/BlurryPhotoSeeker.git
