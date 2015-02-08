<?php

$db = mysql_connect("192.185.34.202", "ramendev_jctwood", "M1n1flam3") 
	or die('Failed to connect: ' . mysql_error());

mysql_select_db('ramendev_snakeball') or die('Failed to access database');

$query = "SELECT * FROM highscores ORDER by score DESC, timestamp ASC LIMIT 10;";

$result = mysql_query($query) or die('Query failed: ' . mysql_error());

$result_length = mysql_num_rows($result);
 
for($i = 0; $i < $result_length; $i++)
{
     $row = mysql_fetch_array($result);
     echo $row['name'] . "\t" . $row['score'] . "\n";
}

?>