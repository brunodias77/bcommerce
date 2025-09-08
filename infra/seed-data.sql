-- scripts/seed-data.sql
-- =====================================================
-- SEED DATA FOR E-COMMERCE MICROSERVICES
-- =====================================================

-- =====================================================
-- CATALOG SERVICE SEED DATA
-- =====================================================
\c catalog_service;

-- Insert Categories
INSERT INTO catalog.categories (id, name, slug, description, parent_id) VALUES
('550e8400-e29b-41d4-a716-446655440001', 'Eletrônicos', 'eletronicos', 'Produtos eletrônicos e gadgets tecnológicos', NULL),
('550e8400-e29b-41d4-a716-446655440002', 'Roupas', 'roupas', 'Vestuário masculino e feminino', NULL),
('550e8400-e29b-41d4-a716-446655440003', 'Casa e Jardim', 'casa-jardim', 'Produtos para casa, decoração e jardim', NULL),
('550e8400-e29b-41d4-a716-446655440004', 'Livros', 'livros', 'Livros físicos e digitais', NULL),
('550e8400-e29b-41d4-a716-446655440005', 'Esportes', 'esportes', 'Artigos esportivos e equipamentos de fitness', NULL),
-- Subcategorias
('550e8400-e29b-41d4-a716-446655440011', 'Smartphones', 'smartphones', 'Telefones inteligentes de todas as marcas', '550e8400-e29b-41d4-a716-446655440001'),
('550e8400-e29b-41d4-a716-446655440012', 'Laptops', 'laptops', 'Notebooks e laptops para trabalho e jogos', '550e8400-e29b-41d4-a716-446655440001'),
('550e8400-e29b-41d4-a716-446655440013', 'Acessórios', 'acessorios-eletronicos', 'Cabos, carregadores e acessórios eletrônicos', '550e8400-e29b-41d4-a716-446655440001'),
('550e8400-e29b-41d4-a716-446655440021', 'Masculino', 'roupas-masculino', 'Roupas masculinas', '550e8400-e29b-41d4-a716-446655440002'),
('550e8400-e29b-41d4-a716-446655440022', 'Feminino', 'roupas-feminino', 'Roupas femininas', '550e8400-e29b-41d4-a716-446655440002'),
('550e8400-e29b-41d4-a716-446655440031', 'Móveis', 'moveis', 'Móveis para casa', '550e8400-e29b-41d4-a716-446655440003'),
('550e8400-e29b-41d4-a716-446655440032', 'Decoração', 'decoracao', 'Itens decorativos', '550e8400-e29b-41d4-a716-446655440003');

-- Insert Products
INSERT INTO catalog.products (id, name, slug, description, short_description, sku, price, compare_price, category_id, brand, weight, dimensions, is_featured) VALUES
-- Smartphones
('660e8400-e29b-41d4-a716-446655440001', 'iPhone 15 Pro 128GB', 'iphone-15-pro-128gb', 
 'O iPhone 15 Pro mais avançado da Apple com chip A17 Pro, sistema de câmera Pro e design em titânio. Tela Super Retina XDR de 6,1 polegadas com ProMotion e Always-On display.',
 'iPhone 15 Pro com chip A17 Pro e câmera Pro avançada',
 'PHONE-APPLE-IP15P-128', 4999.90, 5499.90, '550e8400-e29b-41d4-a716-446655440011', 'Apple', 0.187, 
 '{"width": 70.6, "height": 146.6, "depth": 8.25}', true),

('660e8400-e29b-41d4-a716-446655440002', 'Samsung Galaxy S24 Ultra 256GB', 'samsung-galaxy-s24-ultra-256gb',
 'Samsung Galaxy S24 Ultra com S Pen integrada, tela Dynamic AMOLED 2X de 6,8 polegadas, câmera de 200MP e processador Snapdragon 8 Gen 3.',
 'Galaxy S24 Ultra com S Pen e câmera de 200MP',
 'PHONE-SAMSUNG-S24U-256', 4799.90, 5299.90, '550e8400-e29b-41d4-a716-446655440011', 'Samsung', 0.233,
 '{"width": 79.0, "height": 162.3, "depth": 8.6}', true),

('660e8400-e29b-41d4-a716-446655440003', 'Xiaomi Redmi Note 13 Pro 128GB', 'xiaomi-redmi-note-13-pro-128gb',
 'Xiaomi Redmi Note 13 Pro com tela AMOLED de 6,67 polegadas, câmera tripla de 200MP e bateria de 5000mAh com carregamento rápido de 67W.',
 'Redmi Note 13 Pro com câmera de 200MP e bateria de 5000mAh',
 'PHONE-XIAOMI-RN13P-128', 1299.90, 1499.90, '550e8400-e29b-41d4-a716-446655440011', 'Xiaomi', 0.199,
 '{"width": 74.24, "height": 161.15, "depth": 7.98}', false),

-- Laptops
('660e8400-e29b-41d4-a716-446655440004', 'MacBook Air M3 13" 256GB', 'macbook-air-m3-13-256gb',
 'MacBook Air com chip M3 da Apple, tela Liquid Retina de 13,6 polegadas, 8GB de RAM e SSD de 256GB. Até 18 horas de bateria.',
 'MacBook Air M3 com até 18 horas de bateria',
 'LAPTOP-APPLE-MBA-M3-256', 8999.90, 9999.90, '550e8400-e29b-41d4-a716-446655440012', 'Apple', 1.24,
 '{"width": 215.0, "height": 304.1, "depth": 11.3}', true),

('660e8400-e29b-41d4-a716-446655440005', 'Dell Inspiron 15 3000 i5 8GB 512GB', 'dell-inspiron-15-3000-i5-8gb-512gb',
 'Notebook Dell Inspiron 15 3000 com processador Intel Core i5 de 11ª geração, 8GB de RAM, SSD de 512GB e tela de 15,6 polegadas Full HD.',
 'Dell Inspiron 15 com Intel Core i5 e SSD 512GB',
 'LAPTOP-DELL-INS15-I5-512', 2799.90, 3199.90, '550e8400-e29b-41d4-a716-446655440012', 'Dell', 1.83,
 '{"width": 238.0, "height": 354.3, "depth": 19.9}', false),

-- Roupas
('660e8400-e29b-41d4-a716-446655440006', 'Camiseta Básica Masculina 100% Algodão', 'camiseta-basica-masculina-algodao',
 'Camiseta básica masculina confeccionada em 100% algodão pré-encolhido. Disponível nas cores branca, preta, cinza e azul marinho. Gola careca reforçada.',
 'Camiseta básica masculina 100% algodão',
 'CLOTH-MEN-TSHIRT-BASIC', 39.90, 59.90, '550e8400-e29b-41d4-a716-446655440021', 'BasicWear', 0.15,
 '{"width": 50, "height": 70, "depth": 2}', false),

('660e8400-e29b-41d4-a716-446655440007', 'Jeans Skinny Feminino Mid Rise', 'jeans-skinny-feminino-mid-rise',
 'Calça jeans feminina modelo skinny com cintura média. Tecido com elastano para maior conforto e modelagem perfeita. Lavagem escura clássica.',
 'Jeans skinny feminino com elastano',
 'CLOTH-WOMEN-JEANS-SKINNY', 129.90, 179.90, '550e8400-e29b-41d4-a716-446655440022', 'DenimStyle', 0.45,
 '{"width": 35, "height": 110, "depth": 5}', false),

('660e8400-e29b-41d4-a716-446655440008', 'Tênis Esportivo Unissex Running', 'tenis-esportivo-unissex-running',
 'Tênis esportivo unissex ideal para corrida e caminhada. Solado em EVA para maior amortecimento, cabedal respirável e design moderno.',
 'Tênis esportivo para corrida com solado em EVA',
 'SHOES-UNISEX-SNEAKER-RUN', 199.90, 299.90, '550e8400-e29b-41d4-a716-446655440005', 'SportMax', 0.80,
 '{"width": 12, "height": 30, "depth": 20}', true),

-- Casa e Jardim
('660e8400-e29b-41d4-a716-446655440009', 'Sofá 3 Lugares Retrátil e Reclinável', 'sofa-3-lugares-retratil-reclinavel',
 'Sofá de 3 lugares com mecanismo retrátil e reclinável. Estofado em suede amassado, estrutura de madeira maciça e molas ensacadas para maior conforto.',
 'Sofá 3 lugares retrátil e reclinável em suede',
 'FURNITURE-SOFA-3SEAT-REC', 1899.90, 2299.90, '550e8400-e29b-41d4-a716-446655440031', 'HomeComfort', 85.5,
 '{"width": 230, "height": 88, "depth": 90}', true),

('660e8400-e29b-41d4-a716-446655440010', 'Kit 4 Cadeiras Eames Branca', 'kit-4-cadeiras-eames-branca',
 'Kit com 4 cadeiras Eames em polipropileno branco com base de madeira natural. Design moderno e ergonômico, ideais para sala de jantar.',
 'Kit 4 cadeiras Eames brancas com base de madeira',
 'FURNITURE-CHAIR-EAMES-WHITE4', 599.90, 799.90, '550e8400-e29b-41d4-a716-446655440031', 'DesignModern', 12.8,
 '{"width": 46.5, "height": 82, "depth": 55}', false),

-- Livros
('660e8400-e29b-41d4-a716-446655440011', 'Clean Code - Robert C. Martin', 'clean-code-robert-martin',
 'Um dos livros mais importantes sobre programação. Ensina os princípios e práticas de escrita de código limpo, legível e maintível.',
 'Guia essencial para escrever código limpo',
 'BOOK-TECH-CLEANCODE-RCM', 89.90, 119.90, '550e8400-e29b-41d4-a716-446655440004', 'Pearson', 0.65,
 '{"width": 15.5, "height": 23, "depth": 2.8}', true),

('660e8400-e29b-41d4-a716-446655440012', 'O Hobbit - J.R.R. Tolkien', 'o-hobbit-tolkien',
 'A aventura que deu origem ao Senhor dos Anéis. Acompanhe Bilbo Bolseiro em sua jornada épica pela Terra Média.',
 'A aventura épica de Bilbo Bolseiro',
 'BOOK-FANTASY-HOBBIT-TOLKIEN', 34.90, 49.90, '550e8400-e29b-41d4-a716-446655440004', 'HarperCollins', 0.35,
 '{"width": 14, "height": 21, "depth": 2.2}', false);

-- Insert Product Images
INSERT INTO catalog.product_images (product_id, url, alt_text, sort_order) VALUES
-- iPhone 15 Pro images
('660e8400-e29b-41d4-a716-446655440001', 'https://example.com/images/iphone15pro-main.jpg', 'iPhone 15 Pro vista frontal', 0),
('660e8400-e29b-41d4-a716-446655440001', 'https://example.com/images/iphone15pro-back.jpg', 'iPhone 15 Pro vista traseira', 1),
('660e8400-e29b-41d4-a716-446655440001', 'https://example.com/images/iphone15pro-side.jpg', 'iPhone 15 Pro vista lateral', 2),

-- Samsung Galaxy S24 Ultra images
('660e8400-e29b-41d4-a716-446655440002', 'https://example.com/images/galaxy-s24-ultra-main.jpg', 'Samsung Galaxy S24 Ultra vista frontal', 0),
('660e8400-e29b-41d4-a716-446655440002', 'https://example.com/images/galaxy-s24-ultra-spen.jpg', 'Galaxy S24 Ultra com S Pen', 1),

-- MacBook Air M3 images
('660e8400-e29b-41d4-a716-446655440004', 'https://example.com/images/macbook-air-m3-main.jpg', 'MacBook Air M3 vista superior', 0),
('660e8400-e29b-41d4-a716-446655440004', 'https://example.com/images/macbook-air-m3-side.jpg', 'MacBook Air M3 vista lateral', 1),

-- Sofá images
('660e8400-e29b-41d4-a716-446655440009', 'https://example.com/images/sofa-retratil-main.jpg', 'Sofá 3 lugares retrátil', 0),
('660e8400-e29b-41d4-a716-446655440009', 'https://example.com/images/sofa-retratil-extended.jpg', 'Sofá retrátil estendido', 1);

-- Insert Product Attributes
INSERT INTO catalog.product_attributes (product_id, name, value) VALUES
-- iPhone 15 Pro attributes
('660e8400-e29b-41d4-a716-446655440001', 'Cor', 'Titânio Natural'),
('660e8400-e29b-41d4-a716-446655440001', 'Armazenamento', '128GB'),
('660e8400-e29b-41d4-a716-446655440001', 'Tela', '6,1 polegadas'),
('660e8400-e29b-41d4-a716-446655440001', 'Câmera', '48MP Principal + 12MP Ultra Angular + 12MP Telefoto'),
('660e8400-e29b-41d4-a716-446655440001', 'Processador', 'A17 Pro'),

-- Samsung Galaxy S24 Ultra attributes  
('660e8400-e29b-41d4-a716-446655440002', 'Cor', 'Titanium Black'),
('660e8400-e29b-41d4-a716-446655440002', 'Armazenamento', '256GB'),
('660e8400-e29b-41d4-a716-446655440002', 'RAM', '12GB'),
('660e8400-e29b-41d4-a716-446655440002', 'Tela', '6,8 polegadas Dynamic AMOLED 2X'),
('660e8400-e29b-41d4-a716-446655440002', 'Câmera', '200MP Principal'),

-- Camiseta attributes
('660e8400-e29b-41d4-a716-446655440006', 'Tamanho', 'M'),
('660e8400-e29b-41d4-a716-446655440006', 'Cor', 'Branco'),
('660e8400-e29b-41d4-a716-446655440006', 'Material', '100% Algodão'),
('660e8400-e29b-41d4-a716-446655440006', 'Gola', 'Careca'),

-- Jeans attributes
('660e8400-e29b-41d4-a716-446655440007', 'Tamanho', '38'),
('660e8400-e29b-41d4-a716-446655440007', 'Cor', 'Azul Escuro'),
('660e8400-e29b-41d4-a716-446655440007', 'Modelo', 'Skinny'),
('660e8400-e29b-41d4-a716-446655440007', 'Composição', '98% Algodão, 2% Elastano'),

-- Sofá attributes
('660e8400-e29b-41d4-a716-446655440009', 'Lugares', '3'),
('660e8400-e29b-41d4-a716-446655440009', 'Cor', 'Bege'),
('660e8400-e29b-41d4-a716-446655440009', 'Material', 'Suede Amassado'),
('660e8400-e29b-41d4-a716-446655440009', 'Mecanismo', 'Retrátil e Reclinável'),

-- Clean Code attributes
('660e8400-e29b-41d4-a716-446655440011', 'Idioma', 'Português'),
('660e8400-e29b-41d4-a716-446655440011', 'Páginas', '425'),
('660e8400-e29b-41d4-a716-446655440011', 'Editora', 'Pearson'),
('660e8400-e29b-41d4-a716-446655440011', 'Ano', '2019');

-- =====================================================
-- INVENTORY SERVICE SEED DATA
-- =====================================================
\c inventory_service;

-- Insert Stock for all products
INSERT INTO inventory.stock (product_id, available_quantity, reserved_quantity, min_quantity, max_quantity) VALUES
-- Smartphones - Higher stock for popular items
('660e8400-e29b-41d4-a716-446655440001', 50, 0, 5, 200),  -- iPhone 15 Pro
('660e8400-e29b-41d4-a716-446655440002', 30, 0, 5, 150),  -- Samsung Galaxy S24
('660e8400-e29b-41d4-a716-446655440003', 75, 0, 10, 300), -- Xiaomi Redmi Note

-- Laptops - Medium stock
('660e8400-e29b-41d4-a716-446655440004', 25, 0, 3, 100),  -- MacBook Air M3
('660e8400-e29b-41d4-a716-446655440005', 40, 0, 5, 150),  -- Dell Inspiron

-- Roupas - High stock
('660e8400-e29b-41d4-a716-446655440006', 200, 0, 20, 1000), -- Camiseta Básica
('660e8400-e29b-41d4-a716-446655440007', 80, 0, 10, 400),   -- Jeans Feminino
('660e8400-e29b-41d4-a716-446655440008', 120, 0, 15, 500),  -- Tênis Esportivo

-- Móveis - Lower stock (bulk items)
('660e8400-e29b-41d4-a716-446655440009', 8, 0, 1, 50),      -- Sofá 3 Lugares
('660e8400-e29b-41d4-a716-446655440010', 15, 0, 2, 80),     -- Kit Cadeiras Eames

-- Livros - Medium stock
('660e8400-e29b-41d4-a716-446655440011', 60, 0, 5, 300),    -- Clean Code
('660e8400-e29b-41d4-a716-446655440012', 45, 0, 5, 200);    -- O Hobbit

-- Insert initial stock movements (IN)
INSERT INTO inventory.stock_movements (product_id, movement_type, quantity, reference_type, notes) VALUES
('660e8400-e29b-41d4-a716-446655440001', 'IN', 50, 'INITIAL', 'Estoque inicial iPhone 15 Pro'),
('660e8400-e29b-41d4-a716-446655440002', 'IN', 30, 'INITIAL', 'Estoque inicial Galaxy S24 Ultra'),
('660e8400-e29b-41d4-a716-446655440003', 'IN', 75, 'INITIAL', 'Estoque inicial Redmi Note 13 Pro'),
('660e8400-e29b-41d4-a716-446655440004', 'IN', 25, 'INITIAL', 'Estoque inicial MacBook Air M3'),
('660e8400-e29b-41d4-a716-446655440005', 'IN', 40, 'INITIAL', 'Estoque inicial Dell Inspiron 15'),
('660e8400-e29b-41d4-a716-446655440006', 'IN', 200, 'INITIAL', 'Estoque inicial Camiseta Básica'),
('660e8400-e29b-41d4-a716-446655440007', 'IN', 80, 'INITIAL', 'Estoque inicial Jeans Feminino'),
('660e8400-e29b-41d4-a716-446655440008', 'IN', 120, 'INITIAL', 'Estoque inicial Tênis Esportivo'),
('660e8400-e29b-41d4-a716-446655440009', 'IN', 8, 'INITIAL', 'Estoque inicial Sofá 3 Lugares'),
('660e8400-e29b-41d4-a716-446655440010', 'IN', 15, 'INITIAL', 'Estoque inicial Kit Cadeiras Eames'),
('660e8400-e29b-41d4-a716-446655440011', 'IN', 60, 'INITIAL', 'Estoque inicial Clean Code'),
('660e8400-e29b-41d4-a716-446655440012', 'IN', 45, 'INITIAL', 'Estoque inicial O Hobbit');

-- =====================================================
-- NOTIFICATION SERVICE SEED DATA
-- =====================================================
\c notification_service;

-- Insert Notification Templates
INSERT INTO notifications.templates (id, name, type, subject, body, variables) VALUES
('770e8400-e29b-41d4-a716-446655440001', 'welcome_email', 'EMAIL', 
 'Bem-vindo ao E-commerce!', 
 'Olá #{customerName},

Bem-vindo ao nosso e-commerce! Estamos muito felizes em tê-lo como nosso cliente.

Aproveite nossas ofertas especiais para novos clientes:
- Frete grátis na primeira compra
- 10% de desconto com o cupom: BEMVINDO10

Explore nossos produtos e encontre tudo o que você precisa.

Atenciosamente,
Equipe E-commerce',
 '{"customerName": "Nome do cliente"}'),

('770e8400-e29b-41d4-a716-446655440002', 'order_confirmation', 'EMAIL',
 'Pedido Confirmado - #{orderNumber}',
 'Olá #{customerName},

Seu pedido #{orderNumber} foi confirmado com sucesso!

Detalhes do pedido:
- Número: #{orderNumber}
- Valor total: R$ #{totalAmount}
- Data: #{orderDate}

Itens do pedido:
#{orderItems}

Endereço de entrega:
#{shippingAddress}

Você pode acompanhar o status do seu pedido através do nosso site.

Obrigado pela sua compra!
Equipe E-commerce',
 '{"customerName": "Nome do cliente", "orderNumber": "Número do pedido", "totalAmount": "Valor total", "orderDate": "Data do pedido", "orderItems": "Lista de itens", "shippingAddress": "Endereço de entrega"}'),

('770e8400-e29b-41d4-a716-446655440003', 'order_shipped', 'EMAIL',
 'Pedido Enviado - #{orderNumber}',
 'Olá #{customerName},

Ótimas notícias! Seu pedido #{orderNumber} foi enviado e está a caminho.

Informações de envio:
- Código de rastreamento: #{trackingCode}
- Transportadora: #{carrier}
- Previsão de entrega: #{estimatedDelivery}

Você pode acompanhar a entrega através do código de rastreamento.

Atenciosamente,
Equipe E-commerce',
 '{"customerName": "Nome do cliente", "orderNumber": "Número do pedido", "trackingCode": "Código de rastreamento", "carrier": "Transportadora", "estimatedDelivery": "Previsão de entrega"}'),

('770e8400-e29b-41d4-a716-446655440004', 'payment_failed', 'EMAIL',
 'Problema no Pagamento - #{orderNumber}',
 'Olá #{customerName},

Houve um problema com o pagamento do seu pedido #{orderNumber}.

Motivo: #{failureReason}

Para finalizar sua compra, você pode:
1. Tentar um novo pagamento através do nosso site
2. Utilizar outro método de pagamento
3. Entrar em contato conosco para suporte

Não se preocupe, seus itens continuam reservados por 24 horas.

Atenciosamente,
Equipe E-commerce',
 '{"customerName": "Nome do cliente", "orderNumber": "Número do pedido", "failureReason": "Motivo da falha"}'),

('770e8400-e29b-41d4-a716-446655440005', 'order_delivered', 'EMAIL',
 'Pedido Entregue - #{orderNumber}',
 'Olá #{customerName},

Seu pedido #{orderNumber} foi entregue com sucesso!

Esperamos que você esteja satisfeito com sua compra. 

Que tal avaliar os produtos que você comprou? Sua opinião é muito importante para nós e para outros clientes.

Se precisar de suporte pós-venda ou tiver alguma dúvida, estamos sempre aqui para ajudar.

Obrigado por escolher nosso e-commerce!
Equipe E-commerce',
 '{"customerName": "Nome do cliente", "orderNumber": "Número do pedido"}'),

('770e8400-e29b-41d4-a716-446655440006', 'low_stock_alert', 'EMAIL',
 'Alerta: Estoque Baixo - #{productName}',
 'Alerta de Estoque

O produto #{productName} (SKU: #{productSku}) está com estoque baixo:
- Quantidade disponível: #{availableQuantity}
- Quantidade mínima: #{minQuantity}

Por favor, verifique a necessidade de reposição.

Sistema de Gestão',
 '{"productName": "Nome do produto", "productSku": "SKU do produto", "availableQuantity": "Quantidade disponível", "minQuantity": "Quantidade mínima"}'),

-- SMS Templates
('770e8400-e29b-41d4-a716-446655440007', 'order_confirmation_sms', 'SMS',
 NULL,
 'E-commerce: Pedido #{orderNumber} confirmado! Valor: R$ #{totalAmount}. Acompanhe pelo site.',
 '{"orderNumber": "Número do pedido", "totalAmount": "Valor total"}'),

('770e8400-e29b-41d4-a716-446655440008', 'order_shipped_sms', 'SMS',
 NULL,
 'E-commerce: Pedido #{orderNumber} enviado! Código rastreamento: #{trackingCode}',
 '{"orderNumber": "Número do pedido", "trackingCode": "Código de rastreamento"}'),

-- Push Notification Templates  
('770e8400-e29b-41d4-a716-446655440009', 'flash_sale_push', 'PUSH',
 'Flash Sale - 50% OFF!',
 'Não perca! Flash Sale com até 50% de desconto em produtos selecionados. Oferta válida por tempo limitado!',
 '{}'),

('770e8400-e29b-41d4-a716-446655440010', 'abandoned_cart_push', 'PUSH',
 'Carrinho Esquecido',
 'Você esqueceu alguns itens no seu carrinho. Finalize sua compra e ganhe frete grátis!',
 '{"customerName": "Nome do cliente"}');

-- =====================================================
-- SAMPLE ORDERS FOR DEVELOPMENT
-- =====================================================
\c order_service;

-- Sample addresses
INSERT INTO orders.customer_addresses (id, user_id, street, number, complement, neighborhood, city, state, zip_code, is_default) VALUES
('880e8400-e29b-41d4-a716-446655440001', '11111111-1111-1111-1111-111111111111', 'Rua das Flores', '123', 'Apto 45', 'Jardim Botânico', 'São Paulo', 'SP', '01234-567', true),
('880e8400-e29b-41d4-a716-446655440002', '22222222-2222-2222-2222-222222222222', 'Av. Paulista', '1000', 'Conjunto 1201', 'Bela Vista', 'São Paulo', 'SP', '01310-100', true),
('880e8400-e29b-41d4-a716-446655440003', '11111111-1111-1111-1111-111111111111', 'Rua do Trabalho', '456', NULL, 'Centro', 'São Paulo', 'SP', '01234-890', false);

-- Sample orders
INSERT INTO orders.orders (id, order_number, user_id, status, subtotal, shipping_cost, tax_amount, discount_amount, total_amount, shipping_address_id, payment_status, payment_method) VALUES
('990e8400-e29b-41d4-a716-446655440001', '202400000001', '11111111-1111-1111-1111-111111111111', 'DELIVERED', 4999.90, 0.00, 0.00, 499.99, 4499.91, '880e8400-e29b-41d4-a716-446655440001', 'PAID', 'CREDIT_CARD'),
('990e8400-e29b-41d4-a716-446655440002', '202400000002', '22222222-2222-2222-2222-222222222222', 'PROCESSING', 169.80, 15.90, 0.00, 0.00, 185.70, '880e8400-e29b-41d4-a716-446655440002', 'PAID', 'PIX'),
('990e8400-e29b-41d4-a716-446655440003', '202400000003', '11111111-1111-1111-1111-111111111111', 'PENDING', 89.90, 12.90, 0.00, 8.99, 93.81, '880e8400-e29b-41d4-a716-446655440001', 'PENDING', NULL);

-- Sample order items
INSERT INTO orders.order_items (order_id, product_id, product_name, product_sku, quantity, unit_price, total_price) VALUES
-- Order 1 items
('990e8400-e29b-41d4-a716-446655440001', '660e8400-e29b-41d4-a716-446655440001', 'iPhone 15 Pro 128GB', 'PHONE-APPLE-IP15P-128', 1, 4999.90, 4999.90),

-- Order 2 items
('990e8400-e29b-41d4-a716-446655440002', '660e8400-e29b-41d4-a716-446655440006', 'Camiseta Básica Masculina 100% Algodão', 'CLOTH-MEN-TSHIRT-BASIC', 2, 39.90, 79.80),
('990e8400-e29b-41d4-a716-446655440002', '660e8400-e29b-41d4-a716-446655440007', 'Jeans Skinny Feminino Mid Rise', 'CLOTH-WOMEN-JEANS-SKINNY', 1, 129.90, 129.90),

-- Order 3 items  
('990e8400-e29b-41d4-a716-446655440003', '660e8400-e29b-41d4-a716-446655440011', 'Clean Code - Robert C. Martin', 'BOOK-TECH-CLEANCODE-RCM', 1, 89.90, 89.90);

-- Sample order status history
INSERT INTO orders.order_status_history (order_id, status, notes) VALUES
-- Order 1 history (completed order)
('990e8400-e29b-41d4-a716-446655440001', 'PENDING', 'Pedido criado'),
('990e8400-e29b-41d4-a716-446655440001', 'CONFIRMED', 'Pagamento aprovado'),
('990e8400-e29b-41d4-a716-446655440001', 'PROCESSING', 'Pedido em separação'),
('990e8400-e29b-41d4-a716-446655440001', 'SHIPPED', 'Enviado via transportadora'),
('990e8400-e29b-41d4-a716-446655440001', 'DELIVERED', 'Entregue ao destinatário'),

-- Order 2 history (in progress)
('990e8400-e29b-41d4-a716-446655440002', 'PENDING', 'Pedido criado'),
('990e8400-e29b-41d4-a716-446655440002', 'CONFIRMED', 'Pagamento via PIX confirmado'),
('990e8400-e29b-41d4-a716-446655440002', 'PROCESSING', 'Em separação no CD'),

-- Order 3 history (pending payment)
('990e8400-e29b-41d4-a716-446655440003', 'PENDING', 'Aguardando pagamento');

-- =====================================================
-- PAYMENT SERVICE SEED DATA
-- =====================================================
\c payment_service;

-- Sample payments
INSERT INTO payments.payments (id, order_id, amount, status, payment_method, provider, provider_payment_id, provider_response) VALUES
('aa0e8400-e29b-41d4-a716-446655440001', '990e8400-e29b-41d4-a716-446655440001', 4499.91, 'SUCCESS', 'CREDIT_CARD', 'STRIPE', 'pi_1234567890', 
 '{"id": "pi_1234567890", "status": "succeeded", "amount": 449991, "currency": "brl", "payment_method": {"card": {"brand": "visa", "last4": "4242"}}}'),
 
('aa0e8400-e29b-41d4-a716-446655440002', '990e8400-e29b-41d4-a716-446655440002', 185.70, 'SUCCESS', 'PIX', 'MERCADOPAGO', 'mp_pix_123456', 
 '{"id": "mp_pix_123456", "status": "approved", "transaction_amount": 185.70, "payment_method_id": "pix"}'),
 
('aa0e8400-e29b-41d4-a716-446655440003', '990e8400-e29b-41d4-a716-446655440003', 93.81, 'PENDING', 'CREDIT_CARD', 'STRIPE', 'pi_pending_001', 
 '{"id": "pi_pending_001", "status": "requires_payment_method", "amount": 9381}');

-- =====================================================
-- CREATE VIEWS FOR REPORTING
-- =====================================================
\c catalog_service;

-- Product catalog view with stock info (will be used by API Gateway for joins)
CREATE VIEW catalog.v_products_full AS
SELECT 
    p.id,
    p.name,
    p.slug,
    p.description,
    p.short_description,
    p.sku,
    p.price,
    p.compare_price,
    p.currency,
    p.brand,
    p.is_active,
    p.is_featured,
    p.created_at,
    c.name as category_name,
    c.slug as category_slug,
    c.id as category_id,
    (SELECT json_agg(json_build_object('url', url, 'alt_text', alt_text, 'sort_order', sort_order) ORDER BY sort_order)
     FROM catalog.product_images WHERE product_id = p.id) as images,
    (SELECT json_agg(json_build_object('name', name, 'value', value))
     FROM catalog.product_attributes WHERE product_id = p.id) as attributes
FROM catalog.products p
JOIN catalog.categories c ON p.category_id = c.id
WHERE p.deleted_at IS NULL AND p.is_active = true;

\c order_service;

-- Order summary view
CREATE VIEW orders.v_order_summary AS
SELECT 
    o.id,
    o.order_number,
    o.user_id,
    o.status,
    o.total_amount,
    o.currency,
    o.payment_status,
    o.created_at,
    COUNT(oi.id) as item_count,
    SUM(oi.quantity) as total_quantity,
    ca.street || ', ' || ca.number || ', ' || ca.city || ' - ' || ca.state as shipping_address
FROM orders.orders o
LEFT JOIN orders.order_items oi ON o.id = oi.order_id
LEFT JOIN orders.customer_addresses ca ON o.shipping_address_id = ca.id
WHERE o.deleted_at IS NULL
GROUP BY o.id, o.order_number, o.user_id, o.status, o.total_amount, o.currency, o.payment_status, o.created_at, ca.street, ca.number, ca.city, ca.state;

-- =====================================================
-- PERFORMANCE OPTIMIZATION
-- =====================================================

-- Additional indexes for better query performance
\c catalog_service;
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_products_name_trgm ON catalog.products USING gin(name gin_trgm_ops);
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_products_brand_active ON catalog.products(brand, is_active) WHERE is_active = true;
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_products_price_range ON catalog.products(price, category_id) WHERE is_active = true;

\c order_service;
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_orders_user_status_date ON orders.orders(user_id, status, created_at);
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_orders_payment_status_date ON orders.orders(payment_status, created_at);

\c inventory_service;
CREATE INDEX CONCURRENTLY IF NOT EXISTS idx_stock_product_quantities ON inventory.stock(product_id, available_quantity, reserved_quantity);

-- =====================================================
-- FINAL STATISTICS UPDATE
-- =====================================================
\c catalog_service;
ANALYZE;

\c inventory_service;
ANALYZE;

\c order_service;
ANALYZE;

\c payment_service;
ANALYZE;

\c notification_service;
ANALYZE;