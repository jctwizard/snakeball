<?php

$db = mysql_connect("localhost", "ramendev_jctwood", "M1n1flam3") 
	or die('Failed to connect: ' . mysql_error());

mysql_select_db('ramendev_snakeball') or die('Failed to access database');

$id = mysql_real_escape_string($_GET['id'], $db);

$query = 
   "SELECT COUNT(*) AS rank FROM highscores
  	WHERE ((score > (SELECT score FROM highscores WHERE id = '$id'))
  			or ((score = (SELECT score FROM highscores WHERE id = '$id')) 
  				and (timestamp < (SELECT timestamp FROM highscores WHERE id = '$id'))));";

$result = mysql_query($query) or die('Query failed: ' . mysql_error());

$row = mysql_fetch_array($result);

echo $row['rank'];

?>