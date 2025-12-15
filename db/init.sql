CREATE TABLE IF NOT EXISTS experiences (
                                            id                      SERIAL PRIMARY KEY,
                                            slug                    TEXT NOT NULL UNIQUE,
                                            title                   TEXT NOT NULL,
                                            city                    TEXT NOT NULL,
                                            category_name           TEXT NOT NULL,
                                            price_from              NUMERIC(10,2) NOT NULL,
    rating                  NUMERIC(3,2),
    reviews_count           INT,
    hero_url                TEXT DEFAULT '',
    instant_confirmation    BOOLEAN DEFAULT FALSE,
    free_cancellation       BOOLEAN DEFAULT FALSE,
    skip_the_line           BOOLEAN DEFAULT FALSE,
    guided_tour             BOOLEAN DEFAULT FALSE,
    entrance_fees_included  BOOLEAN DEFAULT FALSE,
    private_tour            BOOLEAN DEFAULT FALSE,
    meal_included           BOOLEAN DEFAULT FALSE
    );
CREATE TABLE IF NOT EXISTS users (
                                     id SERIAL PRIMARY KEY,
                                     email TEXT NOT NULL UNIQUE,
                                     password TEXT NOT NULL
);


INSERT INTO users (email, password)
VALUES ('admin@test.com', 'admin')
    ON CONFLICT (email) DO NOTHING;

CREATE TABLE IF NOT EXISTS experience_details (
                                                  id               SERIAL PRIMARY KEY,
                                                  experience_id    INT NOT NULL REFERENCES experiences(id) ON DELETE CASCADE,
    category         TEXT,
    city             TEXT,
    title            TEXT,
    hero             TEXT DEFAULT '',
    hero_url         TEXT DEFAULT '',
    rating           NUMERIC(3,2),
    rating_text      TEXT,
    reviews          INT,
    languages        TEXT,
    duration         TEXT,
    price            NUMERIC(10,2),
    address          TEXT,
    meeting          TEXT,
    cancel_policy    TEXT,
    valid_until      TIMESTAMP NULL,
    description_html TEXT,
    chips_json       TEXT DEFAULT '[]',
    love_json        TEXT DEFAULT '[]',
    included_json    TEXT DEFAULT '[]',
    remember_json    TEXT DEFAULT '[]',
    more_json        TEXT DEFAULT '[]'
    );

CREATE TABLE IF NOT EXISTS reviews (
                                       id            SERIAL PRIMARY KEY,
                                       experience_id INT NOT NULL REFERENCES experiences(id) ON DELETE CASCADE,
    author        TEXT NOT NULL,
    comment       TEXT NOT NULL,
    rating        INT NOT NULL CHECK (rating BETWEEN 1 AND 5),
    created_at    TIMESTAMP NOT NULL DEFAULT NOW()
    );


INSERT INTO experiences (slug, title, city, category_name, price_from, rating, reviews_count, hero_url,
                         instant_confirmation, free_cancellation, skip_the_line, guided_tour, entrance_fees_included, private_tour, meal_included)
VALUES
    ('entrance-ticket-to-chambord-castle','Entrance ticket to Chambord Castle','Chambord','ATTRACTIONS & GUIDED TOURS',22.00,4.7,110,
     'https://images.musement.com/cover/0072/36/chateau-de-chambord-1-jpg_header-7135742.jpeg?w=1680&q=50',
     TRUE, TRUE, TRUE, FALSE, TRUE, FALSE, FALSE),

    ('loire-valley-day-from-tours-with-azay-le-rideau-villandry','Loire Valley Day from Tours with Visit to Azay le Rideau and Villandry','Tours','ATTRACTIONS & GUIDED TOURS',216.00,4.8,35,
     'https://images.musement.com/cover/0162/95/thumb_16194779_cover_header.jpg?w=1680&q=50',
     TRUE, TRUE, TRUE, TRUE, TRUE, FALSE, FALSE),

    ('e-bike-tour-to-chambord-from-villesavin','E‑Bike Tour to Chambord from Villesavin','Chambord','ATTRACTIONS & GUIDED TOURS',197.00,4.6,18,
     'https://images.musement.com/cover/0163/15/thumb_16214539_cover_header.jpg?w=1680&q=50',
     TRUE, TRUE, TRUE, TRUE, TRUE, FALSE, TRUE),

    ('e-bike-tour-to-chambord-from-tours','E‑bike Tour to Chambord from Tours','Tours','ATTRACTIONS & GUIDED TOURS',200.00,4.6,16,
     'https://images.musement.com/cover/0163/15/thumb_16214539_cover_header.jpg?w=1680&q=50',
     TRUE, TRUE, TRUE, TRUE, TRUE, FALSE, TRUE),

    ('full-day-tour-of-chambord-and-chenonceau-from-tours','Full Day Tour of Chambord and Chenonceau from Tours','Tours','ATTRACTIONS & GUIDED TOURS',204.00,4.8,28,
     'https://images.musement.com/cover/0163/25/thumb_16224996_cover_header.jpg?w=1680&q=50',
     TRUE, TRUE, FALSE, TRUE, TRUE, FALSE, FALSE),

    ('retro-sidecar-tour-by-night','Retro sidecar tour by night','Tours','ACTIVITIES',233.00,4.6,20,
     'https://images.musement.com/cover/0142/79/thumb_14178772_cover_header.jpeg?w=1680&q=50',
     TRUE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE),

    ('great-escape-sidecar-tour-from-tours','Great Escape sidecar tour from Tours','Tours','ACTIVITIES',233.00,4.8,19,
     'https://images.musement.com/cover/0163/14/thumb_16213737_cover_header.jpg?w=1680&q=50',
     TRUE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE),

    ('retro-classic-sidecar-tour-from-tours','Retro Classic sidecar tour from Tours','Tours','ACTIVITIES',117.00,4.7,12,
     'https://images.musement.com/cover/0163/14/thumb_16213739_cover_header.jpg?lossless=false&auto=format&fit=crop&h=155&w=280.734375 1x, https://images.musement.com/cover/0163/14/thumb_16213739_cover_header.jpg?lossless=false&auto=format&fit=crop&h=310&w=561.46875 2x',
     TRUE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE),

    ('half-day-sidecar-tour-of-the-loire-valley-from-tours','Half‑day sidecar tour of the Loire Valley from Tours','Tours','ACTIVITIES',467.00,4.9,15,
     'https://images.musement.com/cover/0163/14/thumb_16213747_cover_header.jpg?w=1680&q=50',
     TRUE, TRUE, FALSE, FALSE, FALSE, TRUE, FALSE),

    ('chenonceau-and-chambord-tour-with-wine-tasting','Chenonceau and Chambord Tour with Wine‑Tasting from Tours or Amboise','Tours','ATTRACTIONS & GUIDED TOURS',244.00,4.6,91,
     'https://images.musement.com/cover/0001/74/thumb_73756_cover_header.jpeg?w=1680&q=50',
     TRUE, TRUE, TRUE, TRUE, TRUE, FALSE, FALSE),

    ('guided-visit-villandry-azay-le-rideau-chateaux-from-tours','Guided visit Villandry & Azay‑le‑Rideau Châteaux from Tours','Tours','ATTRACTIONS & GUIDED TOURS',116.00,4.6,22,
     'https://images.musement.com/cover/0163/02/thumb_16201125_cover_header.jpg?w=1680&q=50',
     TRUE, TRUE, TRUE, TRUE, TRUE, FALSE, FALSE),

    ('a-day-in-chambord-and-chenonceau-with-private-lunch','A day in Chambord and Chenonceau with private lunch from Tours','Tours','ATTRACTIONS & GUIDED TOURS',250.00,4.8,17,
     'https://images.musement.com/cover/0163/25/thumb_16224996_cover_header.jpg?w=1680&q=50',
     TRUE, TRUE, TRUE, TRUE, TRUE, FALSE, TRUE),

    ('afternoon-wine-tour-to-vouvray','Afternoon wine tour to Vouvray','Tours','ATTRACTIONS & GUIDED TOURS',104.00,4.9,50,
     'https://images.musement.com/cover/0001/65/thumb_64023_cover_header.png?w=1680&q=50',
     TRUE, TRUE, FALSE, TRUE, FALSE, FALSE, FALSE),

    ('entrance-ticket-for-zooparc-de-beauval','Entrance ticket for ZooParc de Beauval','Saint‑Aignan','TICKETS & EVENTS',48.00,4.6,1300,
     'https://images.musement.com/cover/0165/48/thumb_16447200_cover_header.jpg?w=1680&q=50',
     TRUE, FALSE, TRUE, FALSE, TRUE, FALSE, FALSE);


INSERT INTO experience_details
(experience_id, category, city, title, hero, hero_url, rating, rating_text, reviews, languages, duration, price, cancel_policy, description_html,
 chips_json, love_json, included_json, remember_json, more_json)
SELECT
    e.id, e.category_name, e.city, e.title, e.hero_url, e.hero_url, e.rating, 'Excellent',
    COALESCE(e.reviews_count,0), 'English', 'Flexible', e.price_from,
    'Free cancellation up to 24 hours in advance.',
    '<p>See highlights of the Loire Valley with expert local guides.</p>',
    '[]','[]','[]','[]','[]'
FROM experiences e;


UPDATE experience_details d
SET
    languages = 'English, Italian, French, Spanish, German, Portuguese, Russian, Dutch, Japanese, Polish, Chinese, Korean',
    duration  = 'Flexible',
    address   = 'Château de Chambord, Château, 41250 Chambord, France',
    meeting   = 'Present your ticket at the Chambord Castle entrance (8‑minute walk from the car park).',
    valid_until = '2027-05-18 00:00:00',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Skip the line","Entrance fees included"]',
    love_json  = '["Admire the biggest and most magnificent castle of the Loire Valley","See the Da Vinci‑inspired masterpiece","Avoid the ticket line and spend more time with the art"]',
    included_json = '["Entrance to the garden and the castle"]',
    remember_json = '["Free admission: EU citizens under 26, children under 18, people with disabilities + companion (tickets at the counter)","Open all year except Jan 1, 3rd Thu of March and Dec 25"]',
    description_html = '<p>More than a castle, Château de Chambord is a glorious historical place that will take you to the heart of the Loire Valley. Built in 1519, it is the largest château in the Loire Valley with over 400 rooms, 300 fireplaces and 85 staircases, including the famous double helix.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='entrance-ticket-to-chambord-castle';

UPDATE experience_details d
SET
    languages = 'English',
    duration  = '9h',
    address   = 'Office de Tourisme & des Congrès Tours Loire Valley, 78‑82 Rue Bernard Palissy, 37000 Tours, France',
    meeting   = 'Present your voucher at Tours Tourist Office 10 minutes before departure.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 24 hours before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Skip the line","Entrance fees included","Guided tour","Smaller group size"]',
    love_json  = '["Explore the interior of Azay‑le‑Rideau château","Discover the elegant gardens of Villandry","Visit a winery and taste Vouvray AOC wines"]',
    included_json = '["Guided tour","Wine tasting","Transportation in air‑conditioned vehicle","Skip‑the‑line tickets"]',
    remember_json = '["Children under 4 years old are not allowed","Tour requires a minimum of 2 participants","Wear comfortable shoes"]',
    description_html = '<p>Complete discovery of the Loire with must‑see Azay‑le‑Rideau and Villandry. In the afternoon, learn about Loire Valley wines during a cellar visit in the Vouvray appellation.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='loire-valley-day-from-tours-with-azay-le-rideau-villandry';

UPDATE experience_details d
SET
    languages = 'English',
    duration  = '7h (25 km ride)',
    address   = 'Château de Villesavin, 41250 Tour‑en‑Sologne, France',
    meeting   = 'Meet your guide at Château de Villesavin 10 minutes before departure.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 24 hours before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Skip the line","Entrance fees included","Guided tour","Local touch"]',
    love_json  = '["Visit Château de Chambord, jewel of the Renaissance","Discover the charming family‑owned Château de Villesavin","Ride through the Forest of Chambord","Taste local specialties"]',
    included_json = '["E‑bike + helmet","Guided tour","Skip‑the‑line tickets","Food tasting","Transportation where needed"]',
    remember_json = '["Be used to riding a bike and in good physical condition","Minimum height 155 cm; not suitable for children under 10","In case of heavy rain, replaced by a minibus tour","Wear suitable clothing and sports shoes"]',
    description_html = '<p>Enjoy a scenic e‑bike loop starting from Villesavin and reaching Chambord via forest paths and small roads. Visit both châteaux and sample local specialties.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='e-bike-tour-to-chambord-from-villesavin';

UPDATE experience_details d
SET
    languages = 'English',
    duration  = '8h',
    address   = 'Tours Tourist Office, 78‑82 Rue Bernard Palissy, 37000 Tours, France',
    meeting   = 'Meet your guide in front of Tours Tourist Office 10 minutes before departure.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 24 hours before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Skip the line","Entrance fees included","Guided tour"]',
    love_json  = '["Visit Château de Chambord (UNESCO)","Discover Château de Villesavin","Enjoy a 25 km ride in the Forest of Chambord","Taste local specialties"]',
    included_json = '["E‑bike + helmet","Guided tour","Skip‑the‑line tickets","Food tasting","Transfers if required"]',
    remember_json = '["Wear suitable clothing and comfortable sport shoes","Tour may be replaced by a minibus in case of heavy rain","Good physical condition required"]',
    description_html = '<p>Cycle from Tours to Chambord with electric bikes. Discover Villesavin on the way and savor a tasting of local produce.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='e-bike-tour-to-chambord-from-tours';

UPDATE experience_details d
SET
    languages = 'English',
    duration  = '9.5h',
    address   = 'Tours Tourist Office, 78‑82 Rue Bernard Palissy, 37000 Tours, France',
    meeting   = 'Present your voucher to your guide in front of Tours Tourist Office 10 minutes before departure.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 24 hours before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Skip the line","Entrance fees included","Guided tour","Smaller group size"]',
    love_json  = '["Visit Château de Chenonceau and its gardens","Explore the impressive royal Château de Chambord","Option to enjoy a picnic in a beautiful park (lunch on your own)"]',
    included_json = '["Entrance fees","Guided tour","Transportation in air‑conditioned vehicle"]',
    remember_json = '["A ticket is required for all participants including children and infants","Not accessible for children under 4 years old"]',
    description_html = '<p>Two Loire superstars in one day: the elegant river‑spanning Chenonceau and the monumental Chambord. Small group with an expert driver‑guide.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='full-day-tour-of-chambord-and-chenonceau-from-tours';

UPDATE experience_details d
SET
    languages = 'English, French',
    duration  = '1h30',
    address   = 'Tourist office – Office de Tourisme Tours Val de Loire, 78‑82 Rue Bernard Palissy, 37000 Tours, France',
    meeting   = 'Meet your local rider at the entrance of the tourist office.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 5 days before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Private tour","Local touch"]',
    love_json  = '["Discover Tours by night with a local sider","Step back in time in a vintage sidecar","Stop for a wine‑tasting break under a starry sky"]',
    included_json = '["Private tour in vintage sidecar","Local rider/guide","Wine tasting break"]',
    remember_json = '["Price is per sidecar (1–2 passengers)","Up to 2 sidecars per booking"]',
    description_html = '<p>Nighttime ride through Tours with stories, viewpoints and a convivial tasting stop.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='retro-sidecar-tour-by-night';

UPDATE experience_details d
SET
    languages = 'English, French',
    duration  = '1h30',
    address   = 'Tourist office – Office de Tourisme Tours Val de Loire, 78‑82 Rue Bernard Palissy, 37000 Tours, France',
    meeting   = 'Meet your local rider at the entrance of the tourist office.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 5 days before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Private tour","Local touch"]',
    love_json  = '["Explore the heart of the Loire Valley on a vintage sidecar","Ride between vineyards and châteaux","Custom route with your local sider"]',
    included_json = '["Private tour in vintage sidecar","Local rider/guide"]',
    remember_json = '["Price is per sidecar (1–2 passengers)","Up to 2 sidecars per booking"]',
    description_html = '<p>Unforgettable ride through vineyards and backroads around Tours, tailored by your sider.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='great-escape-sidecar-tour-from-tours';

UPDATE experience_details d
SET
    languages = 'English',
    duration  = '1h',
    address   = 'Tourist office – Office de Tourisme Tours Val de Loire, 78‑82 Rue Bernard Palissy, 37000 Tours, France',
    meeting   = 'Meet your local rider at the entrance of the tourist office.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 5 days before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Private tour","Local touch"]',
    love_json  = '["Discover Tours with a gentleman local sider","Vintage sidecar ride through unusual places and stories"]',
    included_json = '["Private tour in vintage sidecar","Local rider/guide"]',
    remember_json = '["Price is per sidecar (1–2 passengers)","Up to 2 sidecars per booking"]',
    description_html = '<p>Compact introduction to Tours and its highlights with a retro flair.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='retro-classic-sidecar-tour-from-tours';

UPDATE experience_details d
SET
    languages = 'English, French',
    duration  = '4h',
    address   = 'Tourist office – Office de Tourisme Tours Val de Loire, 78‑82 Rue Bernard Palissy, 37000 Tours, France',
    meeting   = 'Meet your local rider at the entrance of the tourist office.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 5 days before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Private tour","Local touch"]',
    love_json  = '["See the best of the Loire Valley in half a day","Ride along the Cher and Indre rivers","Visit châteaux and wineries with your sider"]',
    included_json = '["Private custom tour","Local rider/guide","Vintage sidecar"]',
    remember_json = '["Price is per sidecar (1–2 passengers)","Up to 2 sidecars per type"]',
    description_html = '<p>Half‑day discovery of Loire landscapes, villages and châteaux aboard a sidecar.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='half-day-sidecar-tour-of-the-loire-valley-from-tours';

UPDATE experience_details d
SET
    languages = 'English',
    duration  = '10h',
    address   = 'Meeting in front of Tourist Office of Tours (8:55) or Tourist Office of Amboise (9:25)',
    meeting   = 'Meet your guide at the Tourist Office of Tours at 8:55 AM or Amboise at 9:25 AM.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 24 hours before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Skip the line","Entrance fees included","Guided tour","Small group"]',
    love_json  = '["Visit the two most popular Loire Valley castles","Explore the countryside and its beautiful landscapes","Dive into winemaking from grape to glass and taste local wines"]',
    included_json = '["Entrance tickets to Chenonceau and Chambord","Transportation in 8‑seat minibus","Guide/driver","Wine tasting"]',
    remember_json = '["Mention pickup point during booking","Wear comfortable shoes","Baby seats required for infants/children"]',
    description_html = '<p>Two châteaux, a scenic day on Loire backroads and a friendly tasting—this is the essential Loire experience.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='chenonceau-and-chambord-tour-with-wine-tasting';

UPDATE experience_details d
SET
    languages = 'English',
    duration  = '5.5h',
    address   = 'Tours Tourist Office, 78‑82 Rue Bernard Palissy, 37000 Tours, France',
    meeting   = 'Present your voucher at Tours Tourist Office 10 minutes before departure.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 24 hours before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Skip the line","Entrance fees included","Guided tour","Small group"]',
    love_json  = '["Experience château life of a French family","Discover the elegant Villandry gardens","Enjoy commentary from a local guide"]',
    included_json = '["Châteaux entrance fees","Friendly professional guide","Transfers by minivan"]',
    remember_json = '["Not accessible for children under 4 years old","A ticket is strictly required for each participant","Tour requires a minimum of 2 participants"]',
    description_html = '<p>A compact morning into Renaissance architecture and world‑famous gardens with skip‑the‑line entrances.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='guided-visit-villandry-azay-le-rideau-chateaux-from-tours';

UPDATE experience_details d
SET
    languages = 'English',
    duration  = '9.5h',
    address   = 'Tours Tourist Office, 78‑82 Rue Bernard Palissy, 37000 Tours, France',
    meeting   = 'Meet your guide in front of Tours Tourist Office 10 minutes before departure.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 24 hours before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Skip the line","Entrance fees included","Guided tour","Meal included","Small group"]',
    love_json  = '["Enjoy a delicious local lunch at a lovely private castle","Discover Chenonceau","Visit Chambord with an expert guide"]',
    included_json = '["Entrance to the castles","Traditional lunch with local wines and aperitif","Friendly local guide","Transfers by minivan"]',
    remember_json = '["Not accessible for children under 4 years old","Minimum drinking age is 18 years","Report dietary restrictions up to 48 hours before departure","Tickets strictly required for all participants"]',
    description_html = '<p>Premium full‑day including both châteaux and a convivial private lunch in a charming property.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='a-day-in-chambord-and-chenonceau-with-private-lunch';

UPDATE experience_details d
SET
    languages = 'English',
    duration  = '4.5h',
    address   = 'In front of Tours Tourist Office, 78‑82 Rue Bernard Palissy, 37000 Tours, France',
    meeting   = 'Meet your guide in front of the Tourist Office of Tours.',
    cancel_policy = 'Receive a 100% refund if you cancel up to 24 hours before the experience begins.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Free cancellation","Guided tour","Small group"]',
    love_json  = '["Visit a local winery in the Vouvray region","Learn about winemaking from grape to glass","Explore the famous underground caves"]',
    included_json = '["Friendly professional guide","Transfers by minivan","Wine tasting","Winery tour"]',
    remember_json = '["Must be 18 years of age to drink alcohol","Returns to original departure point","Tickets strictly required for every participant","Not accessible for children under 4 years old"]',
    description_html = '<p>Afternoon immersion into limestone cellars and Chenin Blanc terroirs with generous tastings.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='afternoon-wine-tour-to-vouvray';

UPDATE experience_details d
SET
    languages = 'Self‑guided',
    duration  = 'Flexible (plan 1 full day)',
    address   = 'ZooParc de Beauval, 41110 Saint‑Aignan‑sur‑Cher, France',
    meeting   = 'Present your barcode ticket at the entrance.',
    cancel_policy = 'This product is non‑refundable and cannot be changed or cancelled due to the partner’s policy.',
    chips_json = '["Instant confirmation","Mobile voucher accepted","Skip the line","Entrance fees included"]',
    love_json  = '["Visit one of the most beautiful zoological parks in the world","See nearly 35,000 animals including giant pandas","Great family day with shows and habitats"]',
    included_json = '["Entrance ticket"]',
    remember_json = '["Up to 15 catering places for lunch","Stroller rental, lockers, picnic area and car park available","Free admission for children under 3 years old"]',
    description_html = '<p>World‑class zoological park with immersive exhibits and renowned conservation programs.</p>'
    FROM experiences e WHERE d.experience_id=e.id AND e.slug='entrance-ticket-for-zooparc-de-beauval';

INSERT INTO reviews (experience_id, author, comment, rating, created_at) VALUES
                                                                             ((SELECT id FROM experiences WHERE slug = 'entrance-ticket-to-chambord-castle'),'Anna','Great castle, skip‑the‑line really saved time.',5, NOW() - INTERVAL '5 days'),
                                                                             ((SELECT id FROM experiences WHERE slug = 'loire-valley-day-from-tours-with-azay-le-rideau-villandry'),'Maria','Perfect day trip, guide was amazing.',5, NOW() - INTERVAL '8 days'),
                                                                             ((SELECT id FROM experiences WHERE slug = 'e-bike-tour-to-chambord-from-villesavin'),'John','It was fire!!!!!!!.',5, NOW() - INTERVAL '5 days'),
                                                                            ((SELECT id FROM experiences WHERE slug = 'loire-valley-day-from-tours-with-azay-le-rideau-villandry'),'Maria','Боже дайте 3 52 питер',5, NOW() - INTERVAL '8 days'),
                                                                             ((SELECT id FROM experiences WHERE slug = 'e-bike-tour-to-chambord-from-villesavin'),'John','Оченьььььььь классснооооооо',5, NOW() - INTERVAL '4 days');