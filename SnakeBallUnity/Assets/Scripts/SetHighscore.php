<?php

$db = mysql_connect("192.185.34.202", "ramendev_jctwood", "M1n1flam3") 
	or die('Failed to connect: ' . mysql_error());

mysql_select_db('ramendev_snakeball') or die('Failed to access database');

$id = mysql_real_escape_string($_GET['id'], $db);
$score = mysql_real_escape_string($_GET['score'], $db);

$query = 
   "UPDATE highscores
	SET score = '$score', timestamp = 'CURRENT_TIMESTAMP'
	WHERE id = '$id' and '$score' > score;

	SELECT * FROM highscores
	ORDER BY score DESC;";

$result = mysql_query($query) or die('Query failed: ' . mysql_error());

?>