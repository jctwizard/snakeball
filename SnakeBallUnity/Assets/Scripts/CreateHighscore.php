<?php

$db = mysql_connect("192.185.34.202", "ramendev_jctwood", "M1n1flam3") 
	or die('Failed to connect: ' . mysql_error());

mysql_select_db('ramendev_snakeball') or die('Failed to access database');

$name = mysql_real_escape_string($_GET['name'], $db);
$score = mysql_real_escape_string($_GET['score'], $db);

$query = 
   "INSERT INTO Scores
	SET name = '$name', score = '$score', ts = CURRENT_TIMESTAMP;

	SELECT * FROM highscores
	ORDER BY score DESC;";

$result = mysql_query($query) or die('Query failed: ' . mysql_error());

echo mysql_insert_id($db) . "";

?>