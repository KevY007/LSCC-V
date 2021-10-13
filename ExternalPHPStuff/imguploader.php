<?php
ini_set('display_errors', 0);
ini_set('display_startup_errors', 0);
error_reporting(0);

	$error="none";
	
	if($_FILES['uploader']['error'] !== UPLOAD_ERR_OK) 
	{ 
		$error="Error during upload";
	}
	$extsAllowed = array( 'png', 'jpg', 'jpeg', 'bmp', 'gif' );
	$extUpload = strtolower( substr( strrchr($_FILES['uploader']['name'], '.') ,1) ) ;
	if (!in_array($extUpload, $extsAllowed))
	{
		$error='Invalid file type!';
	}	
	if($_FILES['uploader']['size'] > 20480000) 
	{
		$error='File size must be under 20 MB.';
	}
	
	if($error == "none")
	{ 
		//Upload the file on the server
		
		$name = "screenshots/{$_FILES['uploader']['name']}";
		$result = move_uploaded_file($_FILES['uploader']['tmp_name'], $name);
		
		if($result)
		{
			echo '<script type="text/javascript">',
				'mp.trigger("', $_GET["event"], '", 1, "', $_FILES['uploader']['name'], '", "', $_GET["id"], '");',
				'</script>'
				;
		}
		
	} 
	else 
	{ 
		echo '<script type="text/javascript">',
				'mp.trigger("', $_GET["event"], '", 0, "', $error, '", "', $_GET["id"], '");',
				'</script>'
				;
	}
?>