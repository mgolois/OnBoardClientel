/*
	Prologue by HTML5 UP
	html5up.net | @ajlkn
	Free for personal and commercial use under the CCA 3.0 license (html5up.net/license)
*/

(function($) {

	var	$window = $(window),
		$body = $('body'),
		$nav = $('#nav');

	// Breakpoints.
		breakpoints({
			wide:      [ '961px',  '1880px' ],
			normal:    [ '961px',  '1620px' ],
			narrow:    [ '961px',  '1320px' ],
			narrower:  [ '737px',  '960px'  ],
			mobile:    [ null,     '736px'  ]
		});

	// Play initial animations on page load.
		$window.on('load', function() {
			window.setTimeout(function() {
				$body.removeClass('is-preload');
			}, 100);
		});

	// Nav.
		var $nav_a = $nav.find('a');

		$nav_a
			.addClass('scrolly')
			.on('click', function(e) {

				var $this = $(this);

				// External link? Bail.
					if ($this.attr('href').charAt(0) != '#')
						return;

				// Prevent default.
					e.preventDefault();

				// Deactivate all links.
					$nav_a.removeClass('active');

				// Activate link *and* lock it (so Scrollex doesn't try to activate other links as we're scrolling to this one's section).
					$this
						.addClass('active')
						.addClass('active-locked');

			})
			.each(function() {

				var	$this = $(this),
					id = $this.attr('href'),
					$section = $(id);

				// No section for this link? Bail.
					if ($section.length < 1)
						return;

				// Scrollex.
					$section.scrollex({
						mode: 'middle',
						top: '-10vh',
						bottom: '-10vh',
						initialize: function() {

							// Deactivate section.
								$section.addClass('inactive');

						},
						enter: function() {

							// Activate section.
								$section.removeClass('inactive');

							// No locked links? Deactivate all links and activate this section's one.
								if ($nav_a.filter('.active-locked').length == 0) {

									$nav_a.removeClass('active');
									$this.addClass('active');

								}

							// Otherwise, if this section's link is the one that's locked, unlock it.
								else if ($this.hasClass('active-locked'))
									$this.removeClass('active-locked');

						}
					});

			});

	// Scrolly.
		$('.scrolly').scrolly();

	// Header (narrower + mobile).

		// Toggle.
			$(
				'<div id="headerToggle">' +
					'<a href="#header" class="toggle"></a>' +
				'</div>'
			)
				.appendTo($body);

		// Header.
			$('#header')
				.panel({
					delay: 500,
					hideOnClick: true,
					hideOnSwipe: true,
					resetScroll: true,
					resetForms: true,
					side: 'left',
					target: $body,
					visibleClass: 'header-visible'
				});
		
	var api_url = "https://onboardingclientel.azurewebsites.net/api/"	
	//var api_url = "http://localhost:7071/api/"	

	//GET CLIENTS
	$('#veil').show();
	$.get(api_url + "GetClients", function(data){
		var isPlainAzureFunction  = (getQueryStrings().length == 0);
		console.log((isPlainAzureFunction ? "plain azure function" : "durable function"))
		toastr.info('Successfully loaded client data')
		$.each(data, function(index, client){
			$('#clientRows').append(
				"<tr>" +
				"<td>" + ( client.documentUrl === null ? client.name : "<a target='_blank' href='" + client.documentUrl + "'>" + client.name + "</a>") +"</td>" +
				"<td>"+ client.industry+"</td>" +
				"<td><a href='"+ client.url+"' target='_blank'>"+ client.url+"</a></td>" +
				"<td class='text-center'><span class='icon solid " + (client.documentGenerated === null ? "red fa-times-circle" :"green fa-check-circle") +"'></span></td>" +
				"<td class='text-center'><span class='icon solid " + (client.emailSent === null ? "red fa-times-circle" :"green fa-check-circle") +"'></span></td>" +
				"<td class='text-center'><span class='icon solid " + (client.documentReviewed === null ? "red fa-times-circle" :"green fa-check-circle") +"'></span></td>" +
				"<td></td>" +
				"</tr>");

		});
	}).fail(function(){
		toastr.error(' an error occurred loading all clients');
	}).always(function(){
		$('#veil').hide();
	});	


	// Read a page's GET URL variables and return them as an associative array.
	function getQueryStrings()
	{
		var vars = [], hash;
		if(window.location.href.indexOf('?') == -1)
			return vars;

		var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
		for(var i = 0; i < hashes.length; i++)
		{
			hash = hashes[i].split('=');
			vars.push(hash[0]);
			vars[hash[0]] = hash[1];
		}
		return vars;
	}
	//POST CLIENT

	$('#submitBtn').click(function(){

		$('#veil').show();

		var client = 	{
			name : $('#clientName').val(),
			industry : $('#clientIndustry').val(),
			url : $('#clientUrl').val(),
			comment : $('#clientComment').val()
		};
		var qStrings = getQueryStrings();
		var isPlainAzureFunction  = (qStrings.length == 0);
		console.log((isPlainAzureFunction ? "plain azure function" : "durable function"))
		$.ajax({
			type: 'POST',
			url: api_url + (isPlainAzureFunction ? "RegisterClient" : "NewClient"),
			data: JSON.stringify(client),
			contentType: 'application/json; charset=utf-8',
			dataType: 'json',
			success: function (data) {
				
				console.log(data);
				//Clear input
				$('#clientName').val('');
				$('#clientIndustry').val('');
				$('#clientUrl').val('');
				$('#clientComment').val('');

				if(isPlainAzureFunction){
					$('#clientRows').append(
						"<tr>" +
						"<td>"+ data.name+"</td>" +
						"<td>"+ data.industry+"</td>" +
						"<td><a href='"+ data.url+"' target='_blank'>"+ data.url+"</a></td>" +
						"<td class='text-center'><span class='icon solid " + (data.documentGenerated === null ? "red fa-times-circle" :"green fa-check-circle") +"'></span></td>" +
						"<td class='text-center'><span class='icon solid " + (data.emailSent === null ? "red fa-times-circle" :"green fa-check-circle") +"'></span></td>" +
						"<td class='text-center'><span class='icon solid " + (data.documentReviewed === null ? "red fa-times-circle" :"green fa-check-circle") +"'></span></td>" +
						"<td></td>" +
						"</tr>");
				}
				$("#veil").hide();
				toastr.success('Client successfully saved');
				
			},
			error: function (error) {
					
					$("#veil").hide();
					toastr.error(' an error occurred saving client');
			}
		});

	});

})(jQuery);