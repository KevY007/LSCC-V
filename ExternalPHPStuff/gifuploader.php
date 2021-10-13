<?php
ini_set('display_errors', 0);
ini_set('display_startup_errors', 0);
error_reporting(0);

	require "GifCreator/AnimGif.php";

	$anim = new GifCreator\AnimGif();
	$total = count($_FILES['uploader']['name']);
	$error="none";

	$images = array_fill(0, $total, "null");
	
	for( $i=0 ; $i < $total ; $i++ ) 
	{
		if($_FILES['uploader']['error'][$i] !== UPLOAD_ERR_OK) 
		{ 
			$error.="Error during upload";
		}
		
		$extsAllowed = array( 'jpg', 'jpeg' );
		$extUpload = strtolower( substr( strrchr($_FILES['uploader']['name'][$i], '.') ,1) ) ;
		if (!in_array($extUpload, $extsAllowed))
		{
			$error.='Invalid file type!';
		}	
		
		if($_FILES['uploader']['size'][$i] > 20480000) 
		{
			$error.='Single image size must be under 20 MB.';
		}
		
		$tmpFilePath = $_FILES['uploader']['tmp_name'][$i];
		if ($tmpFilePath == "")
		{
			$error.="Invalid file tmp path.";
		}
		
		if($error == "none")
		{ 
			for($l = 0; $l < $total; $l++)
			{
				if($_POST["file{$l}"] == $_FILES['uploader']['name'][$i])
				{
					$images[$l] = $tmpFilePath;
					break;
				}
			}
		}
	}

	if(count($images) < 1)
	{
		$error.="No files uploaded!";
	}

	if($error == "none")
	{
		
		$anim	-> create($images, 4.1) // ADJUSTED ONLY FOR 24 FPS
			-> save("screenshots/{$_GET['gif']}");
			
		echo '<script type="text/javascript">',
			'mp.trigger("GIFUploadResult", 1, "', $_GET["gif"], '");',
			'</script>'
			;
	}
	else
	{
		echo '<script type="text/javascript">',
					'mp.trigger("GIFUploadResult", 0, "', $error, '");',
					'</script>'
					;
	}

?>
